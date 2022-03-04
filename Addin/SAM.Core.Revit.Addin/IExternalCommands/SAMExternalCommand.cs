using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SAM.Core.Revit.Addin
{
    public abstract class SAMExternalCommand : IExternalCommand
    {
        public abstract string RibbonPanelName { get; }

        public abstract Autodesk.Revit.UI.Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet);

        public abstract void Create(RibbonPanel ribbonPanel);
    }
}
