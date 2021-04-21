using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdatePanelType : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1a975cf1-1df0-4e14-9d52-c900e8d569f8");

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
        public SAMAnalyticalUpdatePanelType()
          : base("SAMAnalytical.UpdatePanelType", "SAManalytical.UpdatePanelType",
              "Modify Update Analytical Construction from csv file heading column: Prefix, Name, Width, Function,SAM_BuildingElementType, template Family. New Name Family,SAM Types in Template ",
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
                result.Add(ParamDefinition.FromParam(new GooPanelParam() { Name = "_panel", NickName = "_panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(ParamDefinition.FromParam(new GooPanelParam() { Name = "panel", NickName = "panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.HostObjectType() { Name = "elementType", NickName = "elementType", Description = "Revit ElementType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            Document document = RhinoInside.Revit. Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Panel panel = null;
            index = Params.IndexOfInputParam("_panel");
            if (index == -1 || !dataAccess.GetData(index, ref panel))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(document);

            int index_Panel = Params.IndexOfOutputParam("panel");
            int index_ElementType = Params.IndexOfOutputParam("elementType");

            Geometry.Spatial.Vector3D normal = panel.Normal;

            PanelType panelType = panel.PanelType;
            if (panelType == PanelType.Air || panelType == PanelType.Undefined)
            {
                if (index_Panel != -1)
                    dataAccess.SetData(index_Panel, new GooPanel(new Panel(panel)));
             
                if (index_ElementType != -1)
                    dataAccess.SetData(index_ElementType, Analytical.Revit.Convert.ToRevit_HostObjAttributes(panel, document, new Core.Revit.ConvertSettings(false, true, false)));
                
                return;
            }

            PanelType panelType_Normal = Analytical.Revit.Query.PanelType(normal);
            if(panelType_Normal == PanelType.Undefined || panelType.PanelGroup() == panelType_Normal.PanelGroup())
            {
                if (index_Panel != -1)
                    dataAccess.SetData(index_Panel, new GooPanel(new Panel(panel)));

                if (index_ElementType != -1)
                    dataAccess.SetData(index_ElementType, Analytical.Revit.Convert.ToRevit_HostObjAttributes(panel, document, new Core.Revit.ConvertSettings(false, true, false)));
                
                return;
            }

            if(panelType.PanelGroup() == PanelGroup.Floor || panelType.PanelGroup() == PanelGroup.Roof)
            {
                double value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldY);
                if (Math.Abs(value) <= Core.Revit.Tolerance.Tilt)
                {
                    if (index_Panel != -1)
                        dataAccess.SetData(index_Panel, new GooPanel(new Panel(panel)));

                    if (index_ElementType != -1)
                        dataAccess.SetData(index_ElementType, Analytical.Revit.Convert.ToRevit_HostObjAttributes(panel, document, new Core.Revit.ConvertSettings(false, true, false)));

                    return;
                }
            }

            HostObjAttributes hostObjAttributes = Analytical.Revit.Modify.DuplicateByType(document, panelType_Normal, panel.Construction) as HostObjAttributes;
            if (hostObjAttributes == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Skipped - Could not duplicate construction for {0} panel (Guid: {1}).", panel.Name, panel.Guid));
                return;
            }

            if (index_Panel != -1)
                dataAccess.SetData(index_Panel, new Panel(panel, panelType_Normal));

            if (index_ElementType != -1)
                dataAccess.SetData(index_ElementType, hostObjAttributes);

        }
    }
}