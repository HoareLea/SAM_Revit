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
    public class CreateFloorPlan : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "Project Setup";

        public override int Index => 9;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

        public override string Text => "Create\nFloor Plans";

        public override string ToolTip => "Create Floor Plans";

        public override string AvailabilityClassName => null;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if(document == null)
            {
                return Result.Failed;
            }

            List<ViewPlan> viewPlans = new FilteredElementCollector(document).OfClass(typeof(ViewPlan)).Cast<ViewPlan>().ToList();
            if(viewPlans == null || viewPlans.Count == 0)
            {
                return Result.Failed;
            }

            ViewPlan viewPlan = null;
            using (Core.Windows.Forms.ComboBoxForm<ViewPlan> comboBoxForm = new Core.Windows.Forms.ComboBoxForm<ViewPlan>("Select ViewPlan", viewPlans, (ViewPlan x) => x.Name, viewPlans.Find(x => x.Id.IntegerValue == 312)))
            {
                if(comboBoxForm.ShowDialog() != DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                viewPlan = comboBoxForm.SelectedItem;
            }

            if(viewPlan == null)
            {
                MessageBox.Show("Could not find view to be duplicated");
                return Result.Failed;
            }

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if(levels == null || levels.Count == 0)
            {
                MessageBox.Show("Could not find levels");
                return Result.Failed;
            }

            using (Transaction transaction = new Transaction(document, "Create Views"))
            {
                transaction.Start();
                List<ViewPlan> result = Core.Revit.Modify.DuplicateViewPlan(viewPlan, levels, true);
                transaction.Commit();
            }

            return Result.Succeeded;
        }
    }
}
