using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalRevitSetUpperLimit : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2e88344e-24d3-4300-828b-8d128d5d25ec");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRevitSetUpperLimit()
          : base("SAMAnalytical.RevitSetUpperLimit", "SAMAnalytical.RevitSetUpperLimit",
              "Sets Upper Limit of Space or Wall",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "_element", "_element", "Revit Element", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "Element", "Element", "Revit Element", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Parameters Updated", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            Element element = null;
            if (!dataAccess.GetData(0, ref element))
                return;

            if(element == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool successful = false;
            if(element is Wall)
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
                            Parameter parameter = element.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                            if (parameter != null)
                            {
                                successful = parameter.SetValue(level.Id);
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
                    Level level = levels.Find(x => x.Elevation.AlmostEqual(((Autodesk.Revit.DB.Mechanical.Space)element).UnboundedHeight, Core.Tolerance.MacroDistance));
                    if (level != null)
                    {
                        Parameter parameter = element.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL);
                        if (parameter != null)
                        {
                            if (parameter.AsElementId() != level.Id)
                            {
                                successful = parameter.SetValue(level.Id);
                                parameter = element.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                                if (parameter != null)
                                    parameter.Set(0.0);
                            }
                                
                        }
                    }
                }
            }

            dataAccess.SetData(0, element);
            dataAccess.SetData(1, successful);
        }
    }
}