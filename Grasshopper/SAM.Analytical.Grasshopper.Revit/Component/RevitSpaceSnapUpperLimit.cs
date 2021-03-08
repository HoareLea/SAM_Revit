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
    public class RevitSpaceSnapUpperLimit : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5f99e864-b6a3-4958-bf34-f709dd24532b");

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
        public RevitSpaceSnapUpperLimit()
          : base("Revit.SpaceSnapUpperLimit", "Revit.SpaceSnapUpperLimit",
              "Snap Upper Limit of Space",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.SpatialElement(), "_space", "_space", "Revit Space", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.SpatialElement(), "Space", "Space", "Revit Space", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Parameters Updated", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            Autodesk.Revit.DB.Mechanical.Space space = null;
            if (!dataAccess.GetData(0, ref space) || space == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool successful = false;

            List<Autodesk.Revit.DB.Mechanical.Space> spaces = new FilteredElementCollector(space.Document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
            spaces?.RemoveAll(x => x.Id == space.Id || x.Level.Elevation <= space.Level.Elevation);

            Level level = null;

            if(spaces != null || spaces.Count != 0)
            {
                List<Geometry.Planar.Face2D> face2Ds = Geometry.Revit.Query.BoundaryFace2Ds(space);
                if(face2Ds != null && face2Ds.Count > 0)
                {
                    List<Level> levels_Temp = new List<Level>();
                    foreach (Autodesk.Revit.DB.Mechanical.Space space_Temp in spaces)
                    {
                        List<Geometry.Planar.Face2D> face2Ds_Temp = Geometry.Revit.Query.BoundaryFace2Ds(space_Temp);
                        if (face2Ds_Temp == null && face2Ds_Temp.Count == 0)
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
                level = Core.Revit.Query.HighLevel(space.Level);

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

            dataAccess.SetData(0, space);
            dataAccess.SetData(1, successful);
        }
    }
}