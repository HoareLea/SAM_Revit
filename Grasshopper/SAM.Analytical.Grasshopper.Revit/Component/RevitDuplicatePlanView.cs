using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitDuplicatePlanView : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f4e701fd-37e2-430d-9b7f-4315476854db");

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
        public RevitDuplicatePlanView()
          : base("Revit.DuplicatePlanView", "Revit.DuplicatePlanView",
              "Duplicate PlanView by given ViewPlan in Levels",
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.View() { Name = "_viewPlan", NickName = "_viewPlan", Description = "Revit ViewPlan", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.View() { Name = "levels_", NickName = "levels_", Description = "Revit Levels", Access = GH_ParamAccess.list, Optional = true }, ParamRelevance.Occasional));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "useExisting_", NickName = "useExisting_", Description = "Use Existing views and create missing only", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(new ParamDefinition(param_Boolean, ParamRelevance.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.View() { Name = "viewPlans", NickName = "viewPlans", Description = "Revit ViewPlans", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
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
            index = Params.IndexOfInputParam("_viewPlan");
            if (index == -1 || !dataAccess.GetData(index, ref view) || !(view is ViewPlan))
                return;

            ViewPlan viewPlan = (ViewPlan)view;

            if(viewPlan == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Level> levels = new List<Level>();
            index = Params.IndexOfInputParam("levels_");
            if (index == -1 || !dataAccess.GetDataList(index, levels))
                levels = null;

            bool useExisting = true;
            index = Params.IndexOfInputParam("useExisting_");
            if (index == -1 || !dataAccess.GetData(index, ref useExisting))
                return;

            StartTransaction(viewPlan.Document);

            List<ViewPlan> result = Core.Revit.Modify.DuplicateViewPlan(viewPlan, levels, useExisting);

            index = Params.IndexOfOutputParam("viewPlans");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}