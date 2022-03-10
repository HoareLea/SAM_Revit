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
    public class AddWallTags : SAMExternalCommand
    {
        public override string RibbonPanelName => "Project Setup";

        public override int Index => 12;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData.Application.ActiveUIDocument.Document;

            List<View> views = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
            if (views == null || views.Count == 0)
            {
                return Result.Failed;
            }

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_WallTags).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
            {
                return Result.Failed;
            }

            List<Autodesk.Revit.DB.Wall> walls = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Autodesk.Revit.DB.Wall)).Cast<Autodesk.Revit.DB.Wall>().ToList();
            if (walls == null || walls.Count == 0)
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

            double minLength = 1;
            using (Core.Windows.Forms.TextBoxForm<double> textBoxForm = new Core.Windows.Forms.TextBoxForm<double>("Wall Length", "Min Wall Length"))
            {
                textBoxForm.Value = minLength;
                if(textBoxForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                minLength = textBoxForm.Value;
            }

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

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
            minLength = UnitUtils.ConvertToInternalUnits(minLength, DisplayUnitType.DUT_METERS);
#else
            minLength = UnitUtils.ConvertToInternalUnits(minLength, UnitTypeId.Meters);
#endif

            for(int i = walls.Count - 1; i >= 0; i--)
            {
                LocationCurve locationCurve = walls[i]?.Location as LocationCurve;
                if(locationCurve == null)
                {
                    walls.RemoveAt(i);
                    continue;
                }

                Curve curve = locationCurve.Curve;
                if(curve == null)
                {
                    walls.ElementAt(i);
                    continue;
                }

                if(curve.Length < minLength)
                {
                    walls.ElementAt(i);
                    continue;
                }
            }

            List<Tuple<ElementId, List<Autodesk.Revit.DB.Wall>>> tuples = new List<Tuple<ElementId, List<Autodesk.Revit.DB.Wall>>>();
            tuples.Add(new Tuple<ElementId, List<Autodesk.Revit.DB.Wall>>(elementTypes.Find(x => x.FamilyName == "Anno_Tag_SAM_CurtainWall")?.Id, walls.FindAll(x => x.WallType.Kind == WallKind.Curtain)));
            tuples.Add(new Tuple<ElementId, List<Autodesk.Revit.DB.Wall>>(elementTypes.Find(x => x.FamilyName == "Anno_Tag_SAM_Wall")?.Id, walls.FindAll(x => x.WallType.Kind != WallKind.Curtain)));

            using (Transaction transaction = new Transaction(document, "Add Wall Tags"))
            {
                using (Core.Windows.SimpleProgressForm simpleProgressForm = new Core.Windows.SimpleProgressForm("Add Wall Tags", string.Empty, tuples.Count + 1))
                {
                    transaction.Start();

                    foreach (Tuple<ElementId, List<Autodesk.Revit.DB.Wall>> tuple in tuples)
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

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Core.Query.FullTypeName(GetType()), "Add\nWall Tags", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Add Wall Tags";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
            pushButton.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
        }
    }
}
