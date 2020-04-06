using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using Autodesk.Revit.DB;

using SAM.Analytical.Grasshopper.Revit.Properties;


namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalDuplicateConstruction : RhinoInside.Revit.GH.Components.TransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7af06904-abe4-4b64-957a-28adfad8a124");

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
              "Duplicate Analytical Object",
              "SAM", "Revit")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;
            
            index = inputParamManager.AddParameter(new GooPanelParam(), "_panel_", "_panel_", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager[index].Optional = false;

            inputParamManager.AddTextParameter("_name_", "_name_", "Name", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ElementType", "ElementType", "Revit ElementType", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
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

            dataAccess.SetData(0, Analytical.Revit.Modify.Duplicate(document, panel.Construction, panel.PanelType ,name));
        }
    }
}