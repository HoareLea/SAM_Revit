using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Core.Revit.Addin.Properties;
using System.Windows.Media.Imaging;

namespace SAM.Core.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Wiki : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "General";

        public override int Index => 0;

        public override BitmapSource BitmapSource => Windows.Convert.ToBitmapSource(Resources.SAM_Small);

        public override string Text => "Info";

        public override string ToolTip => "Info";

        public override string AvailabilityClassName => typeof(AlwaysAvailableExternalCommandAvailability).FullName;

        public override Autodesk.Revit.UI.Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            System.Diagnostics.Process.Start("https://github.com/HoareLea/SAM/wiki/00-Home");

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
