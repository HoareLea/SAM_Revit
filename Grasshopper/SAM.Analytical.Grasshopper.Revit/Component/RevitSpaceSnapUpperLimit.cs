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
    public class RevitSpaceSnapUpperLimit : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5f99e864-b6a3-4958-bf34-f709dd24532b");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSpaceSnapUpperLimit()
          : base("Revit.SpaceSnapUpperLimit", "Revit.SpaceSnapUpperLimit",
              "Snap Upper Limit of Space",
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "_space", NickName = "_space", Description = "Revit Space", Access = GH_ParamAccess.item }, ParamRelevance.Binding));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "space", NickName = "space", Description = "Revit Space", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
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

            Autodesk.Revit.DB.Mechanical.Space space = null;
            index = Params.IndexOfInputParam("_space");
            if (index == -1 || !dataAccess.GetData(index, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool successful = false;

            List<Autodesk.Revit.DB.Mechanical.Space> spaces = new FilteredElementCollector(space.Document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
            spaces?.RemoveAll(x => x.Id == space.Id || x.Level.Elevation <= space.Level.Elevation);

            Level level = null;

            StartTransaction(space.Document);

            if(spaces != null || spaces.Count != 0)
            {
                List<Geometry.Planar.Face2D> face2Ds = Geometry.Revit.Query.BoundaryFace2Ds(space);
                if(face2Ds != null && face2Ds.Count > 0)
                {
                    List<Level> levels_Temp = new List<Level>();
                    foreach (Autodesk.Revit.DB.Mechanical.Space space_Temp in spaces)
                    {
                        List<Geometry.Planar.Face2D> face2Ds_Temp = Geometry.Revit.Query.BoundaryFace2Ds(space_Temp);
                        if (face2Ds_Temp == null || face2Ds_Temp.Count == 0)
                            continue;

                        Level level_Temp = null;
                        foreach(Geometry.Planar.Face2D face2D in face2Ds)
                        {
                            foreach (Geometry.Planar.Face2D face2D_Temp in face2Ds_Temp)
                            {
                                List<Geometry.Planar.Face2D> face2s_Intersection = Geometry.Planar.Query.Intersection(face2D, face2D_Temp);
                                if (face2s_Intersection == null || face2s_Intersection.Count == 0)
                                    continue;

                                face2s_Intersection.RemoveAll( x => x == null || x.GetArea() < Tolerance.MacroDistance);
                                if(face2Ds.Count > 0)
                                {
                                    level_Temp = space_Temp.Level;
                                    break;
                                }
                            }

                            if (level_Temp != null)
                                break;
                        }

                        if (level_Temp != null && levels_Temp.Find(x => x.Id == level_Temp.Id) == null)
                            levels_Temp.Add(level_Temp);
                    }

                    if (levels_Temp != null && levels_Temp.Count != 0)
                    {
                        levels_Temp.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));
                        level = levels_Temp[0];
                    }
                }
            }

            if(level == null)
            {
                BoundingBoxXYZ boundingBoxXYZ = space.get_BoundingBox(null);
                if(boundingBoxXYZ != null)
                {
                    List<Level> levels = new FilteredElementCollector(space.Document).OfClass(typeof(Level)).Cast<Level>().ToList();
                    double elevation = boundingBoxXYZ.Max.Z;
                    if(levels.Find(x => Math.Abs(x.Elevation - elevation) < Tolerance.Distance) != null)
                    {
                        dataAccess.SetData(0, space);
                        dataAccess.SetData(1, false);
                        return;
                    }
                }
                
                level = Core.Revit.Query.HighLevel(space.Level);
            }
                

            if(level != null)
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

            index = Params.IndexOfOutputParam("space");
            if(index != -1)
            dataAccess.SetData(index, space);

            index = Params.IndexOfOutputParam("successful");
            if (index != -1)
                dataAccess.SetData(index, successful);
        }
    }
}