using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LoadAnalyticalModel : Core.Revit.Addin.PushButtonExternalCommand
    {
        public override string RibbonPanelName => "Analytical";

        public override int Index => 7;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_Small);

        public override string Text => "Load\nAnalytical Model";

        public override string ToolTip => "Load Analytical Model";

        public override string AvailabilityClassName => null;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if (document == null)
            {
                return Result.Failed;
            }

            string path = null;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog(Core.Revit.Addin.ExternalApplication.WindowHandle) != DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                path = openFileDialog.FileName;
            }

            if(string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            AnalyticalModel analyticalModel = Core.Convert.ToSAM<AnalyticalModel>(path)?.FirstOrDefault();
            if(analyticalModel == null)
            {
                return Result.Failed;
            }

            List<Architectural.Level> levels = null;

            List<Panel> panels = analyticalModel.GetPanels();
            if(panels != null)
            {
                levels = Architectural.Create.Levels(panels);
            }

            Core.Revit.ConvertSettings convertSettings = new Core.Revit.ConvertSettings(true, true, true);
            convertSettings.AddParameter("AnalyticalModel", analyticalModel);

            using (Transaction transaction = new Transaction(document, "Load Analytical Model"))
            {
                transaction.Start();

                List<Element> elements = new List<Element>();

                using (Core.Windows.SimpleProgressForm simpleProgressForm = new Core.Windows.SimpleProgressForm("Lad Analytical Model", string.Empty, 4))
                {
                    simpleProgressForm.Increment("Creating Levels");

                    foreach (Architectural.Level level in levels)
                    {
                        Level level_Revit = Architectural.Revit.Convert.ToRevit(level, document, convertSettings);
                        if (level_Revit != null)
                        {
                            elements.Add(level_Revit);
                        }
                    }

                    simpleProgressForm.Increment("Creating Model");

                    List<Element> elements_AnalyticalModel = Convert.ToRevit(analyticalModel, document, convertSettings);
                    if (elements_AnalyticalModel != null)
                    {
                        elements.AddRange(elements_AnalyticalModel);
                    }

                    simpleProgressForm.Increment("Coping Parameters");
                    Modify.CopySpatialElementParameters(document, Tool.TAS);

                    simpleProgressForm.Increment("Finishing");
                }

                transaction.Commit();
            }

            return Result.Succeeded;
        }
    }
}
