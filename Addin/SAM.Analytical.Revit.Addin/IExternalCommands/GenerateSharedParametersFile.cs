using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Core.Revit.Addin;
using SAM.Analytical.Revit.Addin.Properties;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System;
using NetOffice.ExcelApi;
using System.Collections.Generic;
using System.Linq;
using SAM.Core.Revit;
using SAM.Core.Windows;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GenerateSharedParametersFile : SAMExternalCommand
    {
        public override string RibbonPanelName => "Shared Parameters";

        public override int Index => 4;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Autodesk.Revit.ApplicationServices.Application application = externalCommandData?.Application?.Application;
            if (application == null)
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

            if (string.IsNullOrEmpty(path_Excel))
            {
                return Result.Failed;
            }

            string path_SharedParametersFile = null;

            using (SaveFileDialog aSaveFileDialog = new SaveFileDialog())
            {
                aSaveFileDialog.Filter = "Text file|*.txt;*.txt";
                aSaveFileDialog.Title = "Select Shared Parameter file";
                if (aSaveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return Result.Cancelled;
                }
                path_SharedParametersFile = aSaveFileDialog.FileName;
            }

            if (string.IsNullOrEmpty(path_SharedParametersFile))
            {
                return Result.Failed;
            }

            System.IO.File.WriteAllText(path_SharedParametersFile, string.Empty);

            Func<Worksheet, bool> func = new Func<Worksheet, bool>((Worksheet worksheet) =>
            {
                if (worksheet == null)
                {
                    return false;
                }

                int index_Group = 2;
                int index_Guid = 1;
                int index_ParameterType = 8;
                int index_Name = 7;

                List<int> indexes = new List<int>() { index_Group, index_Guid, index_ParameterType, index_Name };

                object[,] objects = worksheet.Range(worksheet.Cells[5, 1], worksheet.Cells[worksheet.UsedRange.Rows.Count, indexes.Max()]).Value as object[,];

                object[,] guids = worksheet.Range(worksheet.Cells[5, 1], worksheet.Cells[worksheet.UsedRange.Rows.Count, 1]).Value as object[,];

                if (objects == null || objects.GetLength(0) <= 1 || objects.GetLength(1) < indexes.Max())
                {
                    return false;
                }

                using (SharedParameterFileWrapper sharedParameterFileWrapper = new SharedParameterFileWrapper(application))
                {
                    sharedParameterFileWrapper.Open(path_SharedParametersFile);

                    List<string> names = new List<string>();
                    using (SimpleProgressForm simpleProgressForm = new SimpleProgressForm("Creating Shared Parameters", "Parameter", objects.GetLength(0)))
                    {
                        for (int i = 1; i <= objects.GetLength(0); i++)
                        {
                            if (string.IsNullOrEmpty(objects[i, index_Name] as string) || string.IsNullOrEmpty(objects[i, index_ParameterType] as string))
                            {
                                simpleProgressForm.Increment("???");
                                continue;
                            }

                            string name = objects[i, index_Name] as string;
                            if (string.IsNullOrEmpty(name))
                                simpleProgressForm.Increment("???");
                            else
                                simpleProgressForm.Increment(name);

                            string parameterTypeString = objects[i, index_ParameterType] as string;
                            parameterTypeString = parameterTypeString.Replace(" ", string.Empty);
                            ParameterType parameterType = ParameterType.Invalid;
                            if (Enum.TryParse(parameterTypeString, out parameterType))
                            {
                                name = name.Trim();

                                if (names.IndexOf(name) < 0)
                                {
                                    names.Add(name);
                                    ExternalDefinitionCreationOptions externalDefinitionCreationOptions = new ExternalDefinitionCreationOptions(name, parameterType);
                                    string guid_String = objects[i, index_Guid] as string;
                                    if (!string.IsNullOrEmpty(guid_String))
                                    {
                                        Guid aGuid;
                                        if (Guid.TryParse(guid_String, out aGuid))
                                            externalDefinitionCreationOptions.GUID = aGuid;
                                    }
                                    else
                                    {
                                        externalDefinitionCreationOptions.GUID = Guid.NewGuid();
                                    }

                                    string group = objects[i, index_Group] as string;
                                    if (string.IsNullOrEmpty(group))
                                        group = "General";

                                    ExternalDefinition externalDefinition = sharedParameterFileWrapper.Create(group, externalDefinitionCreationOptions) as ExternalDefinition;
                                    if(externalDefinition != null)
                                    {
                                        guids[i, 1] = externalDefinition.GUID.ToString();
                                    }
                                    
                                }
                            }
                        }
                    }

                    sharedParameterFileWrapper.Close();
                }

                return true;

            });

            Core.Excel.Modify.Update(path_Excel, "Live", func);

            return Result.Succeeded;
        }

        public override void Create(RibbonPanel ribbonPanel)
        {
            BitmapSource bitmapSource = Core.Windows.Convert.ToBitmapSource(Resources.SAM_GenerateSharedParametersFile);

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Core.Query.FullTypeName(GetType()), "Generate", GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = "Generate Shared Parameters File";
            pushButton.LargeImage = bitmapSource;
            pushButton.Image = bitmapSource;
            pushButton.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
        }
    }
}
