using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Core.Revit.Addin;
using SAM.Analytical.Revit.Addin.Properties;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using SAM.Core.Windows;
using System;
using System.Linq;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AddParameters : SAMExternalCommand
    {
        public override string RibbonPanelName => "Shared Parameters";

        public override int Index => 5;

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
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                path_Excel = openFileDialog.FileName;
            }

            if (string.IsNullOrWhiteSpace(path_Excel))
            {
                return Result.Failed;
            }

            object[,] objects = Core.Excel.Query.Values(path_Excel, "Live");
            if (objects == null || objects.GetLength(0) <= 1 || objects.GetLength(1) < 11)
            {
                return Result.Failed;
            }

            string path_SharedParametersFile = externalCommandData.Application.Application.SharedParametersFilename;

            string path_SharedParametersFile_Temp = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(path_SharedParametersFile_Temp, string.Empty);
            externalCommandData.Application.Application.SharedParametersFilename = path_SharedParametersFile_Temp;

            using (Transaction transaction = new Transaction(document, "Add Parameters"))
            {
                transaction.Start();

                using (Core.Revit.SharedParameterFileWrapper sharedParameterFileWrapper = new Core.Revit.SharedParameterFileWrapper(externalCommandData.Application.Application))
                {
                    sharedParameterFileWrapper.Open();

                    BindingMap bindingMap = document.ParameterBindings;
                    List<string> names = new List<string>();

                    using (SimpleProgressForm progressForm = new SimpleProgressForm("Creating Shared Parameters", "Parameter", objects.GetLength(0)))
                    {
                        int index_Group = 2;
                        int index_Guid = 1;
                        int index_ParameterType = 8;
                        int index_Name = 7;

                        for (int i = 1; i <= objects.GetLength(0); i++)
                        {
                            if (!string.IsNullOrEmpty(objects[i, index_Name] as string) && !string.IsNullOrEmpty(objects[i, index_ParameterType] as string))
                            {
                                string name = objects[i, index_Name] as string;
                                if (string.IsNullOrEmpty(name))
                                    progressForm.Increment("???");
                                else
                                    progressForm.Increment(name);

                                string parameterTypeString = objects[i, index_ParameterType] as string;
                                parameterTypeString = parameterTypeString.Replace(" ", string.Empty);
                                ParameterType parameterType = ParameterType.Invalid;
                                if (Enum.TryParse(parameterTypeString, out parameterType))
                                {
                                    name = name.Trim();

                                    if (names.IndexOf(name) < 0)
                                    {
                                        names.Add(name);

                                        Definition definition = sharedParameterFileWrapper.Find(name);
                                        if(definition == null)
                                        {
                                            ExternalDefinitionCreationOptions externalDefinitionCreationOptions = new ExternalDefinitionCreationOptions(name, parameterType);
                                            string guid_String = objects[i, index_Guid] as string;
                                            if (!string.IsNullOrEmpty(guid_String))
                                            {
                                                Guid guid;
                                                if (Guid.TryParse(guid_String, out guid))
                                                {
                                                    externalDefinitionCreationOptions.GUID = guid;
                                                }
                                            }

                                            string parameterGroup = objects[i, index_Group] as string;
                                            if(!string.IsNullOrWhiteSpace(parameterGroup))
                                            {
                                                definition = sharedParameterFileWrapper.Create(parameterGroup, externalDefinitionCreationOptions);
                                            }
                                        }

                                        if(definition != null)
                                        {
                                            if (objects[i, 13] is string && objects[i, 12] is string)
                                            {
                                                string group = objects[i, 12] as string;
                                                BuiltInParameterGroup builtInParameterGroup;
                                                if (Enum.TryParse("PG_" + group, out builtInParameterGroup))
                                                {
                                                    CategorySet categorySet = new CategorySet();

                                                    string[] categoryNames = (objects[i, 13] as string).Split(',');
                                                    foreach (string categoryName in categoryNames)
                                                    {
                                                        if (string.IsNullOrEmpty(categoryName))
                                                            continue;

                                                        BuiltInCategory builtInCategory;
                                                        if (Enum.TryParse("OST_" + categoryName.Trim().Replace(" ", string.Empty), out builtInCategory))
                                                        {
                                                            Category category = document.Settings.Categories.Cast<Category>().ToList().Find(x => x.Id.IntegerValue == (int)builtInCategory);
                                                            if (category != null)
                                                                categorySet.Insert(category);
                                                        }
                                                    }

                                                    if (categorySet.Size > 0)
                                                    {
                                                        string instance = objects[i, 14] as string;

                                                        if (string.IsNullOrEmpty(instance))
                                                            continue;

                                                        Autodesk.Revit.DB.Binding binding = null;
                                                        if (instance != null && instance.Trim().ToUpper() == "INSTANCE")
                                                            binding = externalCommandData.Application.Application.Create.NewInstanceBinding(categorySet);
                                                        else
                                                            binding = externalCommandData.Application.Application.Create.NewTypeBinding(categorySet);

                                                        bindingMap.Insert(definition, binding, builtInParameterGroup);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                progressForm.Increment("???");
                            }
                        }
                    }
                }

                transaction.Commit();
            }

            externalCommandData.Application.Application.SharedParametersFilename = path_SharedParametersFile;
            externalCommandData.Application.Application.OpenSharedParameterFile();

            System.IO.File.Delete(path_SharedParametersFile_Temp);

            return Result.Succeeded;
        }

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Core.Windows.Convert.ToBitmapSource(Resources.SAM_AddParameters, 32, 32);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Core.Query.FullTypeName(GetType()), "Add\nParameters", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Add Parameters";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
        }
    }
}
