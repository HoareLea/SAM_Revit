using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalShellsBySpaces : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("352f8762-3aba-43b8-9bfc-d1024cedafd1");

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
        public SAMAnalyticalShellsBySpaces()
          : base("SAMAnalytical.ShellsBySpaces", "SAMAnalytical.ShellsBySpaces",
              "Gets Shells By Spaces",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            RhinoInside.Revit.GH.Parameters.SpatialElement spatialElement = new RhinoInside.Revit.GH.Parameters.SpatialElement() { Optional = true};
            inputParamManager.AddParameter(spatialElement, "spaces_", "_space_", "Revit Spaces", GH_ParamAccess.list);

            inputParamManager.AddNumberParameter("_offset_", "_offset_", "Offset from bottom of space", GH_ParamAccess.item, 0.1);
            inputParamManager.AddNumberParameter("_snapTolerance_", "_snapTolerance_", "Snap Tolerance", GH_ParamAccess.item, Core.Tolerance.MacroDistance);
            inputParamManager.AddNumberParameter("_tolerance_", "_tolerance_", "Tolerance", GH_ParamAccess.item, Core.Tolerance.Distance);

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new Geometry.Grasshopper.GooSAMGeometryParam(), "Shells", "Shells", "SAM Geometry Shells", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(4, ref run) || !run)
                return;

            List<SpatialElement> spatialElements = new List<SpatialElement>();
            if (!dataAccess.GetDataList(0, spatialElements) || spatialElements == null || spatialElements.Count == 0)
                spatialElements = null;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            double offset = 0.1;
            if (!dataAccess.GetData(1, ref offset))
                offset = 0.1;

            double snapTolerance = Core.Tolerance.MacroDistance;
            if (!dataAccess.GetData(2, ref snapTolerance))
                snapTolerance = Core.Tolerance.MacroDistance;

            double tolerance = Core.Tolerance.Distance;
            if (!dataAccess.GetData(3, ref tolerance))
                tolerance = Core.Tolerance.Distance;

            List<Geometry.Spatial.Shell> result = Analytical.Revit.Create.Shells(document, spatialElements?.ConvertAll(x => x as Autodesk.Revit.DB.Mechanical.Space).FindAll(x => x != null), offset, snapTolerance, tolerance);

            dataAccess.SetDataList(0, result);
        }
    }
}