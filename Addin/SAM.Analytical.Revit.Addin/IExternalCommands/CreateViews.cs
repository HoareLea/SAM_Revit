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
    public class CreateViews : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "Project Setup";

        public override int Index => 10;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

        public override string Text => "Create\nViews";

        public override string ToolTip => "Create Views";

        public override string AvailabilityClassName => null;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if(document == null)
            {
                return Result.Failed;
            }

            List<View> views = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
            if (views == null || views.Count == 0)
            {
                return Result.Failed;
            }

            for(int i=views.Count - 1; i >= 0; i--)
            {
                View view = views[i];
                if(view == null)
                {
                    views.RemoveAt(i);
                    continue;
                }

                if(view.ViewType != ViewType.FloorPlan)
                {
                    views.RemoveAt(i);
                    continue;
                }

                if(!view.IsTemplate)
                {
                    views.RemoveAt(i);
                    continue;
                }

            }

            string[] templateNames = new string[] { "RiserCLG", "RiserHTG", "RiserVNT", "ICType", "RefExhaust", "RefSupply", "SAM Model", "NoPeople", "Heating Load", "Cooling Load" };

            using (Core.Windows.Forms.TreeViewForm<View> treeViewForm = new Core.Windows.Forms.TreeViewForm<View>("Select Templates", views, (View view) => view.Name, null, (View view) => templateNames.Contains(view.Name)))
            {
                if(treeViewForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                views = treeViewForm.SelectedItems;
            }

            if(views == null || views.Count == 0)
            {
                return Result.Failed;
            }

            using (Transaction transaction = new Transaction(document, "Create Views"))
            {
                transaction.Start();
                Core.Revit.Modify.DuplicateViews(document, "00_Reference", views.ConvertAll(x => x.Name), new ViewType[] { ViewType.FloorPlan });
                transaction.Commit();
            }

            return Result.Succeeded;
        }
    }
}
