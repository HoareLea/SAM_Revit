using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit.Addin;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Hydra : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "General";

        public override int Index => 2;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_Hydra, 32, 23);

        public override string Text => "Hydra";

        public override string ToolTip => "Hydra webpage";

        public override string AvailabilityClassName => typeof(AlwaysAvailableExternalCommandAvailability).FullName;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            System.Diagnostics.Process.Start("https://hlhydra.azurewebsites.net/index.html");

            return Result.Succeeded;
        }
    }
}
