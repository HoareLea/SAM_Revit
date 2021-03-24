using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalDuplicateConstruction : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7af06904-abe4-4b64-957a-28adfad8a124");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        private bool run = false;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalDuplicateConstruction()
          : base("SAMAnalytical.DuplicateConstruction", "SAManalytical.DuplicateConstruction",
              "Modify Duplicate Analytical Object",
              "SAM", "Revit")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooPanelParam(), "_panel", "_panel", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item);
            
            index = inputParamManager.AddBooleanParameter("inverted_", "inverted_", "if inverted then name is source type and panel construction is destination type", GH_ParamAccess.item, false);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ElementType", "ElementType", "Revit ElementType", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(3, ref run) || !run)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel) || panel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            if (!dataAccess.GetData(1, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool inverted = false;
            if (!dataAccess.GetData(2, ref inverted))
                inverted = false;

            if (inverted)
                dataAccess.SetData(0, Analytical.Revit.Modify.DuplicateByName(document, name, panel.PanelType, panel.Construction));
            else
                dataAccess.SetData(0, Analytical.Revit.Modify.DuplicateByName(document, panel.Construction, panel.PanelType, name));
        }
    }
}