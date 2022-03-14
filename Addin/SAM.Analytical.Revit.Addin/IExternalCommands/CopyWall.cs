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
    public class CopyWall : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "Tools";

        public override int Index => 18;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_CopyWall, 32, 32);

        public override string Text => "Copy\nWall";

        public override string ToolTip => "Copy Wall from Linked Model";

        public override string AvailabilityClassName => null;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Modify.CopyWall(externalCommandData?.Application?.ActiveUIDocument);

            return Result.Succeeded;
        }
    }
}
