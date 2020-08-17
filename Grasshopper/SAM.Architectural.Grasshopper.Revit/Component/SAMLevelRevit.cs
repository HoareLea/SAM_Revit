using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Architectural.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper.Revit
{
    public class SAMLevelRevit : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b09e4a6e-8dc8-4e0a-8b8f-92e3ccaa8977");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Architectural;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMLevelRevit()
          : base("SAMLevel.Revit", "SAMLevel.Revit",
              "SAM Level to Revit",
              "SAM", "Revit")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooLevelParam(), "_level", "_level", "SAM Architectural Level", GH_ParamAccess.item);
            
            int index = inputParamManager.AddGenericParameter("_convertSettings_", "_convertSettings_", "SAM Convert Settings", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
            
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Level(), "Level", "Level", "Revit Level", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(2, ref run) || !run)
                return;

            ConvertSettings convertSettings = null;
            dataAccess.GetData(1, ref convertSettings);

            if (convertSettings == null)
                convertSettings = Query.ConvertSettings();

            Level level = null;
            if (!dataAccess.GetData(0, ref level))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            Autodesk.Revit.DB.Level level_Revit = Architectural.Revit.Convert.ToRevit(level, document, convertSettings);

            dataAccess.SetData(0, level_Revit);
        }
    }
}