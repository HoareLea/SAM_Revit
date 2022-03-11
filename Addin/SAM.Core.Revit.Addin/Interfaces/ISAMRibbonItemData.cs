using Autodesk.Revit.UI;

namespace SAM.Core.Revit.Addin
{
    public interface ISAMRibbonItemData
    {
        string RibbonPanelName { get; }
        int Index { get; }
        void Create(RibbonPanel ribbonPanel);
    }
}
