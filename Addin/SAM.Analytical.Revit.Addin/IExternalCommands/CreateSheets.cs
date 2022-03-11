using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit.Addin;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateSheets : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "Project Setup";

        public override int Index => 11;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

        public override string Text => "Create\nSheets";

        public override string ToolTip => "Create Sheets";

        public override string AvailabilityClassName => null;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if(document ==null)
            {
                return Result.Failed;
            }

            List<ViewSheet> viewSheets = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().ToList();

            ViewSheet viewSheet = null;
            using (Core.Windows.Forms.ComboBoxForm<ViewSheet> comboBoxForm = new Core.Windows.Forms.ComboBoxForm<ViewSheet>("Reference View Sheet", viewSheets, (ViewSheet x) => string.Format("{0} - {1}", x.SheetNumber, x.Name), viewSheets.Find(x => x.Id.IntegerValue == 725533)) )
            {
                if(comboBoxForm.ShowDialog() != DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                viewSheet = comboBoxForm.SelectedItem;
            }

            if(viewSheet == null)
            {
                return Result.Failed;
            }

            List<ViewPlan> viewPlans = new FilteredElementCollector(document).OfClass(typeof(ViewPlan)).Cast<ViewPlan>().ToList();
            viewPlans?.RemoveAll(x => !x.IsTemplate);
            if(viewPlans == null || viewPlans.Count == 0)
            {
                MessageBox.Show("Could not find Template View Plans");
                return Result.Failed;
            }

            List<string> templateNames = null;
            using (Core.Windows.Forms.TreeViewForm<ViewPlan> treeViewForm = new Core.Windows.Forms.TreeViewForm<ViewPlan>("Select Templates", viewPlans, (ViewPlan x) => x.Name, null,(ViewPlan x) => x.Name == "Cooling Load" || x.Name == "Heating Load"))
            {
                if (treeViewForm.ShowDialog() != DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                templateNames = treeViewForm.SelectedItems?.ConvertAll(x => x.Name);
            }

            using (Transaction transaction = new Transaction(document, "Create Sheets"))
            {
                transaction.Start();
                List<ViewSheet> result = Core.Revit.Create.Sheets(viewSheet, templateNames, true);
                transaction.Commit();
            }

            return Result.Succeeded;
        }
    }
}
