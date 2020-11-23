using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdatePanelType : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1a975cf1-1df0-4e14-9d52-c900e8d569f8");

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
        public SAMAnalyticalUpdatePanelType()
          : base("SAMAnalytical.UpdatePanelType", "SAManalytical.UpdatePanelType",
              "Modify Update Analytical Construction from csv file heading column: Prefix, Name, Width, Function,SAM_BuildingElementType, template Family. New Name Family,SAM Types in Template ",
              "SAM", "Revit")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooPanelParam(), "_panel_", "_panel_", "SAM Analytical Panel", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panel", "Panels", "SAM Analytical Panel", GH_ParamAccess.item);
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObjectType(), "Type", "Type", "Revit Type", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            Document document = RhinoInside.Revit. Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Panel panel = null;
            if (!dataAccess.GetData(0, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = panel.PanelType;
            PanelType panelType_Normal = Analytical.Revit.Query.PanelType(panel.Normal);
            if(panelType == PanelType.Undefined || panelType_Normal == PanelType.Undefined ||  panelType.PanelGroup() == panelType_Normal.PanelGroup())
            {
                dataAccess.SetData(0, new GooPanel(new Panel(panel)));
                dataAccess.SetData(1, Analytical.Revit.Convert.ToRevit_HostObjAttributes(panel, document, new Core.Revit.ConvertSettings(false, true, false)));
                return;
            }

            HostObjAttributes hostObjAttributes = Analytical.Revit.Modify.DuplicateByType(document, panelType_Normal, panel.Construction) as HostObjAttributes;
            if (hostObjAttributes == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Skipped - Could not duplicate construction for {0} panel (Guid: {1}).", panel.Name, panel.Guid));
                return;
            }

            dataAccess.SetData(0, new Panel(panel, panelType_Normal));
            dataAccess.SetData(1, hostObjAttributes);

        }
    }
}