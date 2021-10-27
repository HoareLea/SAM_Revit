using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitSetUpperLimit : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2e88344e-24d3-4300-828b-8d128d5d25ec");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSetUpperLimit()
          : base("Revit.SetUpperLimit", "Revit.SetUpperLimit",
              "Sets Upper Limit of Space or Wall",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Inputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() { Name = "_element", NickName = "_element", Description = "Revit Element", Access = GH_ParamAccess.item }, ParamRelevance.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new ParamDefinition(param_Boolean, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Outputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() { Name = "element", NickName = "element", Description = "Revit Element", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Parameters Updated", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            
            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            Element element = null;
            index = Params.IndexOfInputParam("_element");
            if (index == -1 || !dataAccess.GetData(index, ref element))
                return;

            Transform transform;

            if(element == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(element.Document);

            bool successful = false;
            if(element is Autodesk.Revit.DB.Wall)
            {
                List<Level> levels = new FilteredElementCollector(element.Document).OfClass(typeof(Level)).Cast<Level>().ToList();
                if(levels != null && levels.Count != 0)
                {
                    BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox(null);
                    if(boundingBoxXYZ != null)
                    {
                        Level level = levels.Find(x => x.Elevation.AlmostEqual(boundingBoxXYZ.Max.Z, Core.Tolerance.MacroDistance));
                        if(level != null)
                        {
                            Autodesk.Revit.DB.Wall wall = (Autodesk.Revit.DB.Wall)element;
                            Parameter parameter = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                            if (parameter != null)
                            {
                                successful = parameter.Set(level.Id);
                            }
                                
                        }
                    }
                }
            }
            else if(element is Autodesk.Revit.DB.Mechanical.Space)
            {
                List<Level> levels = new FilteredElementCollector(element.Document).OfClass(typeof(Level)).Cast<Level>().ToList();
                if (levels != null && levels.Count != 0)
                {
                    Autodesk.Revit.DB.Mechanical.Space space = (Autodesk.Revit.DB.Mechanical.Space)element;

                    double elevation = space.UnboundedHeight + levels.Find(x => x.Id == space.LevelId).Elevation;

                    Level level = levels.Find(x => x.Elevation.AlmostEqual(elevation, Core.Tolerance.MacroDistance));
                    if (level != null)
                    {
                        Parameter parameter = space.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL);
                        if (parameter != null)
                        {
                            if (parameter.AsElementId() != level.Id)
                            {
                                successful = parameter.Set(level.Id);
                                parameter = space.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                                if (parameter != null)
                                    parameter.Set(0.0);
                            }
                                
                        }
                    }
                }
            }
            index = Params.IndexOfOutputParam("element");
            if (index != -1)
                dataAccess.SetData(index, element);

            index = Params.IndexOfOutputParam("successful");
            if (index != -1)
                dataAccess.SetData(index, successful);
        }
    }
}