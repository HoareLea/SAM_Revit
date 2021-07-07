using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMCoreCreateSheets : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("65f51465-a92c-4c08-8308-01f0c542447d");

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
        public SAMCoreCreateSheets()
          : base("SAMCore.CreateSheets", "SAMCore.CreateSheets",
              "Create Sheets",
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
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.View() { Name = "_referenceViewSheet", NickName = "_referenceViewSheet", Description = "Elements to be deleted", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_templateNames", NickName = "_templateNames", Description = "View Templates Names", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_matchScopeBox_", NickName = "_matchScopeBox_", Description = "Match Scope Box", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.View() { Name = "viewSheets", NickName = "viewSheets", Description = "Revit View Sheets", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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


            View view = null;
            index = Params.IndexOfInputParam("_referenceViewSheet");
            if (index == -1 || !dataAccess.GetData(index, ref view) || !(view is ViewSheet))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            List<string> templateNames = new List<string>();
            index = Params.IndexOfInputParam("_templateNames");
            if (index == -1 || !dataAccess.GetDataList(index, templateNames))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool matchScopeBox = false;
            index = Params.IndexOfInputParam("_matchScopeBox_");
            if (index == -1 || !dataAccess.GetData(index, ref matchScopeBox))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(view.Document);

            List<ViewSheet> viewSheets = Core.Revit.Create.Sheets((ViewSheet)view, templateNames, matchScopeBox);

            index = Params.IndexOfOutputParam("viewSheets");
            if (index != -1)
                dataAccess.SetDataList(index, viewSheets);
        }
    }
}