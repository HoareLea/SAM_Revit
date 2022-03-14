using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit;
using SAM.Core.Revit.Addin;
using SAM.Core.Tas;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Simulate : PushButtonExternalCommand
    {
        public override string RibbonPanelName => "Tas";

        public override int Index => 17;

        public override BitmapSource BitmapSource => Core.Windows.Convert.ToBitmapSource(Resources.SAM_Simulate, 32, 32);

        public override string Text => "Simulate";

        public override string ToolTip => "Simulate";

        public override string AvailabilityClassName => null;

        public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        {
            Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
            if(document == null)
            {
                return Result.Failed;
            }

            string path = document.PathName;
            if(string.IsNullOrWhiteSpace(path))
            {
                string name = document.Title;
                if(string.IsNullOrWhiteSpace(name))
                {
                    name = "000000_SAM_AnalyticalModel";
                }

                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select Directory";
                    folderBrowserDialog.ShowNewFolderButton = true;
                    if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    {
                        return Result.Cancelled;
                    }

                    path = System.IO.Path.Combine(folderBrowserDialog.SelectedPath, name + ".rvt");
                }

                if(string.IsNullOrWhiteSpace(path))
                {
                    return Result.Failed;
                }

                document.SaveAs(path);
            }

            AnalyticalModel analyticalModel = Convert.ToSAM_AnalyticalModel(document, new ConvertSettings(true, true, false));

            string path_gbXML = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path));
            if(System.IO.File.Exists(path_gbXML))
            {
                System.IO.File.Delete(path_gbXML);
            }

            bool exported = false;


            exported = document.TogbXML(path_gbXML);
            if(!exported)
            {
                return Result.Failed;
            }

            path_gbXML = path_gbXML + ".xml";

            string path_T3D = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".t3d");

            exported = Core.Tas.Convert.ToT3D(path_T3D, path_gbXML, true, true, true, false);
            if (!exported)
            {
                return Result.Failed;
            }

            analyticalModel = Tas.Query.UpdateT3D(analyticalModel, path_T3D);

            //Tas.UpdateWeather Missing !!

            string path_TBD = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".tbd");

            exported = Tas.Convert.ToTBD(path_T3D, path_TBD, 1, 365, 15, true);
            if(!exported)
            {
                return Result.Failed;
            }

            AdjacencyCluster adjacencyCluster = null;

            using (SAMTBDDocument sAMTBDDocument = new SAMTBDDocument(path_TBD))
            {
                Tas.Query.UpdateFacingExternal(analyticalModel, sAMTBDDocument);

                Tas.Modify.AssignAdiabaticConstruction(sAMTBDDocument.TBDDocument.Building, "Adiabatic", new string[] { "-unzoned", "-internal", "-exposed" }, false, true);

                Tas.Modify.UpdateBuildingElements(sAMTBDDocument, analyticalModel);

                adjacencyCluster = analyticalModel.AdjacencyCluster;
                Tas.Modify.UpdateThermalParameters(adjacencyCluster, sAMTBDDocument.TBDDocument?.Building);
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

                Tas.Modify.UpdateZones(analyticalModel, sAMTBDDocument, true);

                //Update.DesignDays Missing!!

                sAMTBDDocument.Save();
            }

            Tas.Query.Sizing(path_TBD, analyticalModel, false, true);

            analyticalModel = Tas.Modify.UpdateDesignLoads(path_TBD, analyticalModel);

            string path_TSD = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".tsd");

            bool result = Tas.Modify.Simulate(path_TBD, path_TSD, 1, 365);

            adjacencyCluster = analyticalModel.AdjacencyCluster;
            Tas.Modify.AddResults(path_TSD, adjacencyCluster);
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            analyticalModel.ToRevit(document, new ConvertSettings(true, true, false));

            return Result.Succeeded;
        }
    }
}
