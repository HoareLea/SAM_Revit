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
    public class Clean : SAMExternalCommand
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

            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_MEPSpaces,
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Lines,
                BuiltInCategory.OST_GenericModel,
                BuiltInCategory.OST_Levels,
                BuiltInCategory.OST_CLines

            };

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));

            List<Element> elements = new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsNotElementType().ToList();

            using (Core.Windows.Forms.TreeViewForm<Element> treeViewForm = new Core.Windows.Forms.TreeViewForm<Element>("Select Elements", elements, (Element x) => string.Format("{0} [{1}]", x.Name, x.Id.IntegerValue), (Element x) => x.Category.Name, (Element x) =>x.Id.IntegerValue != 311))
            {
                treeViewForm.CollapseAll();
                if(treeViewForm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                elements = treeViewForm.SelectedItems;
            }
            using (Transaction transaction = new Transaction(document, "Clean"))
            {
                transaction.Start();

                List<ElementId> elementIds = new List<ElementId>();
                foreach(Element element in elements)
                {
                    if(element == null || !element.IsValidObject)
                    {
                        continue;
                    }

                    try
                    {
                        document.Delete(element.Id);
                    }
                    catch(Exception exception)
                    {
                        elementIds.Add(element.Id);
                    }
                }

                transaction.Commit();
            }

                return Result.Succeeded;
        }

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Core.Query.FullTypeName(GetType()), "Clean", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Clean";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
        }
    }
}
