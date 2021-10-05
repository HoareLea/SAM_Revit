using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Architectural.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper.Revit
{
    public class SAMLevelRevit : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b09e4a6e-8dc8-4e0a-8b8f-92e3ccaa8977");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
        protected override ParamDefinition[] Inputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new GooLevelParam() { Name = "_level", NickName = "_level", Description = "SAM Architectural Level", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                
                global::Grasshopper.Kernel.Parameters.Param_GenericObject param_GenericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_convertSettings_", NickName = "_convertSettings_", Description = "SAM ConvertSettings", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new ParamDefinition(param_GenericObject, ParamRelevance.Occasional));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Level() { Name = "level", NickName = "level", Description = "Revit Level", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
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

            ConvertSettings convertSettings = null;
            index = Params.IndexOfInputParam("_convertSettings_");
            if (index != -1)
                dataAccess.GetData(index, ref convertSettings);

            if (convertSettings == null)
                convertSettings = Core.Revit.Query.ConvertSettings();

            Level level = null;
            index = Params.IndexOfInputParam("_level");
            if (index == -1 || !dataAccess.GetData(index, ref level))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            StartTransaction(document);

            Autodesk.Revit.DB.Level level_Revit = Architectural.Revit.Convert.ToRevit(level, document, convertSettings);

            index = Params.IndexOfOutputParam("level");
            if (index != -1)
                dataAccess.SetData(index, level_Revit);
        }
    }
}