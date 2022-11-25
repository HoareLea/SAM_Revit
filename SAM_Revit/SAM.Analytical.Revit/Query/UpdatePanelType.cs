namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static Panel UpdatePanelType(this Panel panel)
        {
            if (panel == null)
            {
                return null;
            }

            Geometry.Spatial.Vector3D normal = panel.Normal;
            PanelType panelType = panel.PanelType;

            if (panelType == Analytical.PanelType.Air || panelType == Analytical.PanelType.Undefined)
            {
                return Analytical.Create.Panel(panel);
            }

            PanelType panelType_Normal = PanelType(normal);
            if (panelType_Normal == Analytical.PanelType.Undefined || panelType.PanelGroup() == panelType_Normal.PanelGroup())
            {
                return Analytical.Create.Panel(panel);
            }

            if (panelType.PanelGroup() == Analytical.PanelGroup.Floor || panelType.PanelGroup() == Analytical.PanelGroup.Roof)
            {
                double value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldY);
                if (System.Math.Abs(value) <= Core.Revit.Tolerance.Tilt)
                {
                    return Analytical.Create.Panel(panel);
                }
            }

            return Analytical.Create.Panel(panel, panelType_Normal);
        }
    }
}