using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit.Addin;
using SAM.Core.Windows;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RemoveParameters : SAMExternalCommand
    {
        public override string RibbonPanelName => "Shared Parameters";

        public override int Index => 6;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if(document == null)
            {
                return Result.Failed;
            }

            string path_Excel = null;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel Workbook|*.xlsm;*.xlsx";
                openFileDialog.Title = "Select Excel file";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    path_Excel = openFileDialog.FileName;
                }
            }

            if (string.IsNullOrEmpty(path_Excel))
            {
                return Result.Cancelled;
            }

            object[,] objects = Core.Excel.Query.Values(path_Excel, "Live");
            if (objects == null || objects.GetLength(0) <= 1 || objects.GetLength(1) < 11)
            {
                return Result.Failed;
            }

            int index_Group = 2;
            int index_Name = 7;

            List<string> names_Selected = Query.ParameterNames(objects, index_Group, index_Name);
            if (names_Selected == null || names_Selected.Count == 0)
            {
                return Result.Failed;
            }

            using (Transaction transaction = new Transaction(document, "Remove Parameters"))
            {
                transaction.Start();

                BindingMap bindingMap = document.ParameterBindings;

                DefinitionBindingMapIterator definitionBindingMapIterator = bindingMap.ForwardIterator();

                List<Definition> definitions = new List<Definition>();

                while (definitionBindingMapIterator.MoveNext())
                {
                    Definition definition = definitionBindingMapIterator.Key;
                    //Autodesk.Revit.DB.Binding aBinding = aDefinitionBindingMapIterator.Current as Autodesk.Revit.DB.Binding;

                    definitions.Add(definition);
                }

                List<string> names = new List<string>();
                using (SimpleProgressForm simpleProgressForm = new SimpleProgressForm("Creating Shared Parameters", "Parameter", objects.GetLength(0)))
                {
                    for (int i = 5; i <= objects.GetLength(0); i++)
                    {
                        string name = objects[i, index_Name] as string;
                        if (!string.IsNullOrEmpty(name))
                        {
                            Definition definition = definitions.Find(x => x.Name == name);
                            if (definition != null)
                            {
                                bindingMap.Remove(definition);
                            }
                        }
                        else
                        {
                            name = "???";
                        }

                        simpleProgressForm.Increment(name);
                    }
                }

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Core.Windows.Convert.ToBitmapSource(Resources.SAM_RemoveParameters, 32, 32);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Core.Query.FullTypeName(GetType()), "Remove\nParameters", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Remove Parameters";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
        }
    }
}
