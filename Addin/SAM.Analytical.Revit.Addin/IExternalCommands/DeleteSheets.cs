using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit.Addin;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DeleteSheets : SAMExternalCommand
    {
        public override string RibbonPanelName => "Project Setup";

        public override int Index => 15;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if(document == null)
            {
                return Result.Failed;
            }

            List<ViewSheet> viewSheets = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().ToList();
            if (viewSheets == null || viewSheets.Count == 0)
            {
                return Result.Failed;
            }

            List<int> ids = new List<int>() { 725518, 725533, 802983, 805316, 835480, 1007139, 1008572 };

            using (Core.Windows.Forms.TreeViewForm<ViewSheet> treeViewForm = new Core.Windows.Forms.TreeViewForm<ViewSheet>("Select Sheets", viewSheets, (ViewSheet x) => string.Format("{0} - {1}", x.SheetNumber, x.Name), null, (ViewSheet x) => !ids.Contains(x.Id.IntegerValue)))
            {
                if (treeViewForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                viewSheets = treeViewForm.SelectedItems;
            }

            if(viewSheets == null || viewSheets.Count == 0)
            {
                return Result.Failed;
            }

            using (Transaction transaction = new Transaction(document, "Delete Sheets"))
            {
                transaction.Start();

                document.Delete(viewSheets.ConvertAll(x => x.Id));

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Core.Query.FullTypeName(GetType()), "Delete\nSheets", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Delete Sheets";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
        }
    }
}
