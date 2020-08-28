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
    public class SAMAnalyticalCheck : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("34a6666d-9e18-4cf3-bc78-11b954675127");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCheck()
          : base("SAMAnalytical.Check", "SAMAnalytical.Check",
              "Check Document against analytical object",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooLogParam(), "Log", "Log", "SAM Log", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            if (sAMObject is AnalyticalModel)
                sAMObject = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            
            if(!(sAMObject is Panel) && !(sAMObject is Aperture) && !(sAMObject is Space) && !(sAMObject is AdjacencyCluster) && !(sAMObject is AnalyticalModel))
            {
                dataAccess.SetData(0, null);
                return;
            }

            Log log = Analytical.Revit.Create.Log(sAMObject as dynamic, document);

            dataAccess.SetDataList(0, log);
        }
    }
}