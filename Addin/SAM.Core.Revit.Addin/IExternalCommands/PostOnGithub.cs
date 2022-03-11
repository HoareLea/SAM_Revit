using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Core.Revit.Addin.Properties;
using System.Windows.Media.Imaging;

namespace SAM.Core.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PostOnGithub : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "General";

        public override int Index => 1;

        public override BitmapSource BitmapSource => Windows.Convert.ToBitmapSource(Resources.SAM_PostOnGitHub, 32, 32);

        public override string Text => "Post on\nGithub";

        public override string ToolTip => "Post On Github";

        public override string AvailabilityClassName => typeof(AlwaysAvailableExternalCommandAvailability).FullName;

        public override Autodesk.Revit.UI.Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            System.Diagnostics.Process.Start("https://github.com/HoareLea/SAM/issues/new/choose");

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
