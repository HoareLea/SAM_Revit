using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMApertureRevit : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5c1fbd67-9406-4872-b679-49faa4d1132b");

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
        public SAMApertureRevit()
          : base("SAMAperture.Revit", "SAMAperture.Revit",
              "Convert SAM Aperture to Revit Windnow/Door",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooApertureParam(), "_aperture", "_aperture", "SAM Analytical Aperture", GH_ParamAccess.item);

            int index = inputParamManager.AddGenericParameter("_convertSettings_", "_convertSettings_", "SAM Convert Settings", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.FamilyInstance(), "FamilyInstance", "FamilyInstance", "Revit FamilyInstance", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(2, ref run) || !run)
                return;

            ConvertSettings convertSettings = null;
            dataAccess.GetData(1, ref convertSettings);
            convertSettings = this.UpdateSolutionEndEventHandler(convertSettings);

            Aperture aperture = null;
            if (!dataAccess.GetData(0, ref aperture))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            Core.Revit.Modify.RemoveExisting(convertSettings, document, aperture);

            HostObject hostObject = Analytical.Revit.Query.HostObject(aperture, document);
            if(hostObject == null)
            {
                dataAccess.SetData(0, null);
                return;
            }

            FamilyInstance familyInstance_Revit = Analytical.Revit.Convert.ToRevit(aperture, document, hostObject,convertSettings);

            dataAccess.SetData(0, familyInstance_Revit);
        }
    }
}