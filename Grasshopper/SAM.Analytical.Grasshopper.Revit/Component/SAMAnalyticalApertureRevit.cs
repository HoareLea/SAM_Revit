using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalApertureRevit : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5c1fbd67-9406-4872-b679-49faa4d1132b");

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
        public SAMAnalyticalApertureRevit()
          : base("SAMAnalytical.ApertureRevit", "SAMAnalytical.ApertureRevit",
              "Convert SAM Aperture to Revit Windnow/Door",
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
                result.Add(new ParamDefinition(new GooApertureParam() { Name = "_aperture", NickName = "_aperture", Description = "SAM Analytical Aperture", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_convertSettings_", NickName = "_convertSettings_", Description = "SAM ConvertSettings", Optional = true, Access = GH_ParamAccess.item }, ParamRelevance.Occasional));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.FamilyInstance() { Name = "familyInstance", NickName = "familyInstance", Description = "Revit familyInstance", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            int index_familyInstance = -1;

            index_familyInstance = Params.IndexOfOutputParam("familyInstance");
            if (index_familyInstance != -1)
                dataAccess.SetData(index_familyInstance, null);

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            ConvertSettings convertSettings = null;
            index = Params.IndexOfInputParam("_convertSettings_");
            if (index != -1)
                dataAccess.GetData(index, ref convertSettings);

            convertSettings = this.UpdateSolutionEndEventHandler(convertSettings);

            Aperture aperture = null;
            if (!dataAccess.GetData(0, ref aperture))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            StartTransaction(document);

            Core.Revit.Modify.RemoveExisting(convertSettings, document, aperture);

            HostObject hostObject = Analytical.Revit.Query.HostObject(aperture, document);
            if(hostObject == null)
                return;

            FamilyInstance familyInstance_Revit = Analytical.Revit.Convert.ToRevit(aperture, document, hostObject,convertSettings);

            if (index_familyInstance != -1)
                dataAccess.SetData(index_familyInstance, familyInstance_Revit);
        }
    }
}