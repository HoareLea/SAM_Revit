using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Core.Revit.Addin.Properties;
using System.Windows.Media.Imaging;

namespace SAM.Core.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PostOnGithub : SAMExternalCommand
    {
        public override string RibbonPanelName => "General";

        public override int Index => 1;

        public override Autodesk.Revit.UI.Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            System.Diagnostics.Process.Start("https://github.com/HoareLea/SAM/issues/new/choose");

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Windows.Convert.ToBitmapSource(Resources.SAM_PostOnGitHub);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Query.FullTypeName(GetType()), "Post on\nGithub", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Post On Github";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
            pushButton.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
        }
    }
}
