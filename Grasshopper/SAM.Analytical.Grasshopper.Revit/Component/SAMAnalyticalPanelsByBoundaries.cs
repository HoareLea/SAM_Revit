using Autodesk.Revit.DB;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Analytical.Revit;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalPanelsByBoundaries : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fcd537c9-0a80-472c-bc64-07f11c9aa879");

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
        public SAMAnalyticalPanelsByBoundaries()
          : base("SAMAnalytical.PanelsByBoundaries", "SAMAnalytical.PanelsByBoundaries",
              "Creates Wall Panels based on Space Boundaries",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            RhinoInside.Revit.GH.Parameters.SpatialElement spatialElement = new RhinoInside.Revit.GH.Parameters.SpatialElement();
            inputParamManager.AddParameter(spatialElement, "_space", "_space", "Revit Space or Wall", GH_ParamAccess.item);

            RhinoInside.Revit.GH.Parameters.Level level = null;

            level = new RhinoInside.Revit.GH.Parameters.Level();
            level.Optional = true;
            inputParamManager.AddParameter(level, "level_Lower_", "level_Lower_", "Revit Lower Level", GH_ParamAccess.item);

            level = new RhinoInside.Revit.GH.Parameters.Level();
            level.Optional = true;
            inputParamManager.AddParameter(level, "level_Upper_", "level_Upper_", "Revit Upper Level", GH_ParamAccess.item);


            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(3, ref run) || !run)
                return;

            SpatialElement spatialElement = null;
            if(!dataAccess.GetData(0, ref spatialElement) || spatialElement == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Document document = spatialElement.Document;

            Level level_Low = null;
            dataAccess.GetData(1, ref level_Low);

            Level level_High = null;
            dataAccess.GetData(2, ref level_High);

            if(level_Low == null || level_High == null)
            {
                BoundingBoxXYZ boundingBoxXYZ = spatialElement.get_BoundingBox(null);
                if(boundingBoxXYZ != null)
                {
                    if (level_Low == null)
                        level_Low = Core.Revit.Query.LowLevel(document, boundingBoxXYZ.Min.Z);

                    if (level_High == null)
                        level_High = Core.Revit.Query.HighLevel(document, boundingBoxXYZ.Max.Z);
                }
            }

            if (level_Low == null || level_High == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation_Low = Units.Revit.Convert.ToSI(level_Low.Elevation, UnitType.UT_Length);
            double elevation_High = Units.Revit.Convert.ToSI(level_High.Elevation, UnitType.UT_Length);

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<Panel> result = Analytical.Revit.Create.Panels(spatialElement, elevation_Low, elevation_High, convertSettings);

            dataAccess.SetDataList(0, result.ConvertAll(x => new GooPanel(x)));
        }
    }
}