using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
    public class SAMAnalyticalTrimOrExtendWall : SAMTransactionComponent
    {
        public override Guid ComponentGuid => new Guid("e237e813-ed88-483d-8124-3bb5551b7103");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalTrimOrExtendWall()
          : base("SAMAnalytical.TrimOrExtendWall", "SAMAnalytical.TrimOrExtendWall",
              "Modify Trim Or Extend Unconnected Revit Walls",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObject(), "_walls", "_walls", "Revit Host Walls", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_maxDistance_", "_maxDistance_", "Maximum Distance to Adjust walls", GH_ParamAccess.item, 0.52);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObject(), "Walls", "Walls", "Adjusted Revit Walls", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(2, ref run))
                return;

            if (!run)
                return;

            List<Autodesk.Revit.DB.HostObject> hostObjects = new List<Autodesk.Revit.DB.HostObject>();
            if (!dataAccess.GetDataList(0, hostObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxDistance = double.NaN;
            if (!dataAccess.GetData(1, ref maxDistance))
                return;

            List<Autodesk.Revit.DB.Wall> result = Analytical.Revit.Modify.TrimOrExtendWall(hostObjects.FindAll(x => x is Autodesk.Revit.DB.Wall).Cast<Autodesk.Revit.DB.Wall>(), maxDistance);

            dataAccess.SetDataList(0, result);
        }
    }
}