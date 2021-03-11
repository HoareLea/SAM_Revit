using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitDuplicatePlanView : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f4e701fd-37e2-430d-9b7f-4315476854db");

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
        public RevitDuplicatePlanView()
          : base("Revit.DuplicatePlanView", "Revit.DuplicatePlanView",
              "Duplicate PlanView by given ViewPlan in Levels",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.View(), "_viewPlan", "_viewPlan", "Revit ViewPlan", GH_ParamAccess.item);

            RhinoInside.Revit.GH.Parameters.Level level = new RhinoInside.Revit.GH.Parameters.Level() { Name = "levels_", NickName = "levels_", Description = "Revit Levels", Access = GH_ParamAccess.list, Optional = true };
            inputParamManager.AddParameter(level);

            global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean();
            param_Boolean.SetPersistentData(true);
            inputParamManager.AddParameter(param_Boolean, "useExisting_", "useExisting_", "Use Existing views and create missing only", GH_ParamAccess.item);

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.View(), "ViewPlans", "ViewPlans", "Revit ViewPlans", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(3, ref run) || !run)
                return;

            View view = null;
            if (!dataAccess.GetData(0, ref view) || !(view is ViewPlan))
                return;

            ViewPlan viewPlan = (ViewPlan)view;

            if(viewPlan == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Level> levels = new List<Level>();
            if (!dataAccess.GetDataList(1, levels))
                levels = null;

            bool useExisting = true;
            if (!dataAccess.GetData(2, ref useExisting))
                return;

            List<ViewPlan> result = Core.Revit.Modify.DuplicateViewPlan(viewPlan, levels, useExisting);

            dataAccess.SetDataList(0, result);
        }
    }
}