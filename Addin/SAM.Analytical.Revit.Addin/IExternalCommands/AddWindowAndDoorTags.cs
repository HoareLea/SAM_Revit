using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit.Addin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AddWindowAndDoorTags : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "Project Setup";

        public override int Index => 14;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

        public override string Text => "Add Window\nDoor Tags";

        public override string ToolTip => "Add Window and Door Tags";

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

            for (int i = views.Count - 1; i >= 0; i--)
            {
                View view = views[i];
                if (view == null)
                {
                    views.RemoveAt(i);
                    continue;
                }

                if (view.ViewType != ViewType.FloorPlan)
                {
                    views.RemoveAt(i);
                    continue;
                }

                if (!view.IsTemplate)
                {
                    views.RemoveAt(i);
                    continue;
                }

            }

            List<FamilyInstance> familyInstances_Window = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Windows).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().ToList();
            List<ElementType> elementTypes_WindowTags = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_WindowTags).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();

            List<FamilyInstance> familyInstances_Door = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Doors).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().ToList();
            List<ElementType> elementTypes_DoorTags = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_DoorTags).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();

            List<string> templateNames = new List<string> { "Heating Load" };

            using (Core.Windows.Forms.TreeViewForm<View> treeViewForm = new Core.Windows.Forms.TreeViewForm<View>("Select Templates", views, (View view) => view.Name, null, (View view) => templateNames.Contains(view.Name)))
            {
                if (treeViewForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                templateNames = treeViewForm.SelectedItems?.ConvertAll(x => x.Name);
            }

            if (templateNames == null || templateNames.Count == 0)
            {
                return Result.Failed;
            }

            List<Tuple<ElementId, List<FamilyInstance>>> tuples = new List<Tuple<ElementId, List<FamilyInstance>>>();
            tuples.Add(new Tuple<ElementId, List<FamilyInstance>>(elementTypes_WindowTags.Find(x => x.Id.IntegerValue == 775113)?.Id, familyInstances_Window));
            tuples.Add(new Tuple<ElementId, List<FamilyInstance>>(elementTypes_DoorTags.Find(x => x.Id.IntegerValue == 775351)?.Id, familyInstances_Door));

            using (Transaction transaction = new Transaction(document, "Add Window and Door Tags"))
            {
                using (Core.Windows.SimpleProgressForm simpleProgressForm = new Core.Windows.SimpleProgressForm("Add Window and Door Tags", string.Empty, tuples.Count + 1))
                {
                    transaction.Start();

                    foreach (Tuple<ElementId, List<FamilyInstance>> tuple in tuples)
                    {
                        if (tuple.Item1 == null || tuple.Item2 == null || tuple.Item2.Count == 0)
                        {
                            simpleProgressForm.Increment("???");
                            continue;
                        }

                        simpleProgressForm.Increment((document.GetElement(tuple.Item1) as ElementType)?.FamilyName);

                        List<IndependentTag> independentTags = Core.Revit.Modify.TagElements(document, templateNames, tuple.Item1, tuple.Item2.ConvertAll(x => x.Id), false, TagOrientation.Horizontal, new ViewType[] { ViewType.FloorPlan });
                    }

                    simpleProgressForm.Increment("Finishing");

                    transaction.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
