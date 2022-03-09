using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit.Addin;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateViews : SAMExternalCommand
    {
        public override string RibbonPanelName => "Project Setup";

        public override int Index => 8;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if(document == null)
            {
                return Result.Failed;
            }

            string[] templateNames = new string[] { "RiserCLG", "RiserHTG", "RiserVNT", "ICType", "RefExhaust", "RefSupply", "SAM Model", "NoPeople", "Heating Load", "Cooling Load"};

            using (Transaction transaction = new Transaction(document, "Create Views"))
            {
                transaction.Start();
                Core.Revit.Modify.DuplicateViews(document, "00_Reference", templateNames, new ViewType[] { ViewType.FloorPlan });
                transaction.Commit();
            }

            return Result.Succeeded;
        }

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Core.Query.FullTypeName(GetType()), "Create Views", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Create Views";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
        }
    }
}
