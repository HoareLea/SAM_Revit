using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit;
using SAM.Core.Revit.Addin;
using SAM.Core.Tas;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (document == null)
            {
                return Result.Failed;
            }

            WeatherData weatherData = null;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "epw files (*.epw)|*.epw|TAS TBD files (*.tbd)|*.tbd|TAS TSD files (*.tsd)|*.tsd|TAS TWD files (*.twd)|*.twd|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return Result.Cancelled;
                }

                string path_WeatherData = openFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(path_WeatherData).ToLower().Trim();
                if (string.IsNullOrWhiteSpace(extension))
                {
                    return Result.Failed;
                }

                try
                {
                    if (extension.EndsWith("epw"))
                    {
                        weatherData = Weather.Convert.ToSAM(path_WeatherData);
                    }
                    else
                    {
                        List<WeatherData> weatherDatas = Weather.Tas.Convert.ToSAM_WeatherDatas(path_WeatherData);
                        if (weatherDatas == null || weatherDatas.Count == 0)
                        {
                            return Result.Failed;
                        }

                        if (weatherDatas.Count == 1)
                        {
                            weatherData = weatherDatas[0];
                        }
                        else
                        {
                            weatherDatas.Sort((x, y) => x.Name.CompareTo(y.Name));

                            using (Core.Windows.Forms.ComboBoxForm<WeatherData> comboBoxForm = new Core.Windows.Forms.ComboBoxForm<WeatherData>("Select Weather Data", weatherDatas, (WeatherData x) => x.Name))
                            {
                                if (comboBoxForm.ShowDialog() != DialogResult.OK)
                                {
                                    return Result.Cancelled;
                                }

                                weatherData = comboBoxForm.SelectedItem;
                            }

                        }

                    }
                }
                catch (Exception exception)
                {
                    weatherData = null;
                }
            }

            if (weatherData == null)
            {
                return Result.Failed;
            }

            string path = document.PathName;
            if (string.IsNullOrWhiteSpace(path))
            {
                string name = document.Title;
                if (string.IsNullOrWhiteSpace(name))
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

                if (string.IsNullOrWhiteSpace(path))
                {
                    return Result.Failed;
                }

                document.SaveAs(path);
            }

            AnalyticalModel analyticalModel = null;

            using (Core.Windows.SimpleProgressForm simpleProgressForm = new Core.Windows.SimpleProgressForm("Simulate", string.Empty, 9))
            {
                simpleProgressForm.Increment("Converting Model");
                using (Transaction transaction = new Transaction(document, "Convert Model"))
                {
                    transaction.Start();
                    analyticalModel = Convert.ToSAM_AnalyticalModel(document, new ConvertSettings(true, true, false));
                    transaction.RollBack();
                }

                if (analyticalModel == null)
                {
                    MessageBox.Show("Could not convert to AnalyticalModel");
                    return Result.Failed;
                }

                string path_TBD = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".tbd");
                if (System.IO.File.Exists(path_TBD))
                {
                    System.IO.File.Delete(path_TBD);
                }

                List<int> hoursOfYear = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 360, 361, 362, 363, 364, 365, 366, 367, 368, 369, 370, 371, 372, 373, 374, 375, 376, 377, 378, 379, 380, 381, 382, 383, 720, 721, 722, 723, 724, 725, 726, 727, 728, 729, 730, 731, 732, 733, 734, 735, 736, 737, 738, 739, 740, 741, 742, 743, 1080, 1081, 1082, 1083, 1084, 1085, 1086, 1087, 1088, 1089, 1090, 1091, 1092, 1093, 1094, 1095, 1096, 1097, 1098, 1099, 1100, 1101, 1102, 1103, 1440, 1441, 1442, 1443, 1444, 1445, 1446, 1447, 1448, 1449, 1450, 1451, 1452, 1453, 1454, 1455, 1456, 1457, 1458, 1459, 1460, 1461, 1462, 1463, 1800, 1801, 1802, 1803, 1804, 1805, 1806, 1807, 1808, 1809, 1810, 1811, 1812, 1813, 1814, 1815, 1816, 1817, 1818, 1819, 1820, 1821, 1822, 1823, 2160, 2161, 2162, 2163, 2164, 2165, 2166, 2167, 2168, 2169, 2170, 2171, 2172, 2173, 2174, 2175, 2176, 2177, 2178, 2179, 2180, 2181, 2182, 2183, 2520, 2521, 2522, 2523, 2524, 2525, 2526, 2527, 2528, 2529, 2530, 2531, 2532, 2533, 2534, 2535, 2536, 2537, 2538, 2539, 2540, 2541, 2542, 2543, 2880, 2881, 2882, 2883, 2884, 2885, 2886, 2887, 2888, 2889, 2890, 2891, 2892, 2893, 2894, 2895, 2896, 2897, 2898, 2899, 2900, 2901, 2902, 2903, 3240, 3241, 3242, 3243, 3244, 3245, 3246, 3247, 3248, 3249, 3250, 3251, 3252, 3253, 3254, 3255, 3256, 3257, 3258, 3259, 3260, 3261, 3262, 3263, 3600, 3601, 3602, 3603, 3604, 3605, 3606, 3607, 3608, 3609, 3610, 3611, 3612, 3613, 3614, 3615, 3616, 3617, 3618, 3619, 3620, 3621, 3622, 3623, 3960, 3961, 3962, 3963, 3964, 3965, 3966, 3967, 3968, 3969, 3970, 3971, 3972, 3973, 3974, 3975, 3976, 3977, 3978, 3979, 3980, 3981, 3982, 3983, 4320, 4321, 4322, 4323, 4324, 4325, 4326, 4327, 4328, 4329, 4330, 4331, 4332, 4333, 4334, 4335, 4336, 4337, 4338, 4339, 4340, 4341, 4342, 4343, 4680, 4681, 4682, 4683, 4684, 4685, 4686, 4687, 4688, 4689, 4690, 4691, 4692, 4693, 4694, 4695, 4696, 4697, 4698, 4699, 4700, 4701, 4702, 4703, 5040, 5041, 5042, 5043, 5044, 5045, 5046, 5047, 5048, 5049, 5050, 5051, 5052, 5053, 5054, 5055, 5056, 5057, 5058, 5059, 5060, 5061, 5062, 5063, 5400, 5401, 5402, 5403, 5404, 5405, 5406, 5407, 5408, 5409, 5410, 5411, 5412, 5413, 5414, 5415, 5416, 5417, 5418, 5419, 5420, 5421, 5422, 5423, 5760, 5761, 5762, 5763, 5764, 5765, 5766, 5767, 5768, 5769, 5770, 5771, 5772, 5773, 5774, 5775, 5776, 5777, 5778, 5779, 5780, 5781, 5782, 5783, 6120, 6121, 6122, 6123, 6124, 6125, 6126, 6127, 6128, 6129, 6130, 6131, 6132, 6133, 6134, 6135, 6136, 6137, 6138, 6139, 6140, 6141, 6142, 6143, 6480, 6481, 6482, 6483, 6484, 6485, 6486, 6487, 6488, 6489, 6490, 6491, 6492, 6493, 6494, 6495, 6496, 6497, 6498, 6499, 6500, 6501, 6502, 6503, 6840, 6841, 6842, 6843, 6844, 6845, 6846, 6847, 6848, 6849, 6850, 6851, 6852, 6853, 6854, 6855, 6856, 6857, 6858, 6859, 6860, 6861, 6862, 6863, 7200, 7201, 7202, 7203, 7204, 7205, 7206, 7207, 7208, 7209, 7210, 7211, 7212, 7213, 7214, 7215, 7216, 7217, 7218, 7219, 7220, 7221, 7222, 7223, 7560, 7561, 7562, 7563, 7564, 7565, 7566, 7567, 7568, 7569, 7570, 7571, 7572, 7573, 7574, 7575, 7576, 7577, 7578, 7579, 7580, 7581, 7582, 7583, 7920, 7921, 7922, 7923, 7924, 7925, 7926, 7927, 7928, 7929, 7930, 7931, 7932, 7933, 7934, 7935, 7936, 7937, 7938, 7939, 7940, 7941, 7942, 7943, 8280, 8281, 8282, 8283, 8284, 8285, 8286, 8287, 8288, 8289, 8290, 8291, 8292, 8293, 8294, 8295, 8296, 8297, 8298, 8299, 8300, 8301, 8302, 8303, 8640, 8641, 8642, 8643, 8644, 8645, 8646, 8647, 8648, 8649, 8650, 8651, 8652, 8653, 8654, 8655, 8656, 8657, 8658, 8659, 8660, 8661, 8662, 8663 };

                //Run Solar Calculation fo cooling load

                simpleProgressForm.Increment("Solar Calculations");
                SolarCalculator.Modify.Simulate(analyticalModel, hoursOfYear.ConvertAll(x => new DateTime(2018, 1, 1).AddHours(x)), Core.Tolerance.MacroDistance, Core.Tolerance.MacroDistance, 0.012, Core.Tolerance.Distance);

                bool simulate = false;

                List<DesignDay> heatingDesignDays = new List<DesignDay>() { Analytical.Query.HeatingDesignDay(weatherData) };
                List<DesignDay> coolingDesignDays = new List<DesignDay>() { Analytical.Query.CoolingDesignDay(weatherData) };

                using (SAMTBDDocument sAMTBDDocument = new SAMTBDDocument(path_TBD))
                {
                    TBD.TBDDocument tBDDocument = sAMTBDDocument.TBDDocument;

                    simpleProgressForm.Increment("Updating WeatherData");
                    Weather.Tas.Modify.UpdateWeatherData(tBDDocument, weatherData);

                    TBD.Calendar calendar = tBDDocument.Building.GetCalendar();

                    List<TBD.dayType> dayTypes = Query.DayTypes(calendar);
                    if (dayTypes.Find(x => x.name == "HDD") == null)
                    {
                        TBD.dayType dayType = calendar.AddDayType();
                        dayType.name = "HDD";
                    }

                    if (dayTypes.Find(x => x.name == "CDD") == null)
                    {
                        TBD.dayType dayType = calendar.AddDayType();
                        dayType.name = "CDD";
                    }

                    simpleProgressForm.Increment("Converting to TBD");
                    Tas.Convert.ToTBD(analyticalModel, tBDDocument);

                    simpleProgressForm.Increment("Updating Zones");
                    Tas.Modify.UpdateZones(tBDDocument.Building, analyticalModel, true);

                    //simpleProgressForm.Increment("Adding DesignDays");
                    //Tas.Modify.AddDesignDays(tBDDocument, coolingDesignDays, heatingDesignDays, 30);

                    simpleProgressForm.Increment("Updating Shading");
                    simulate = Tas.Modify.UpdateShading(tBDDocument, analyticalModel);

                    sAMTBDDocument.Save();
                }

                SurfaceOutputSpec surfaceOutputSpec = new SurfaceOutputSpec("Tas.Simulate");
                surfaceOutputSpec.SolarGain = true;
                surfaceOutputSpec.Conduction = true;
                surfaceOutputSpec.ApertureData = false;
                surfaceOutputSpec.Condensation = false;
                surfaceOutputSpec.Convection = false;
                surfaceOutputSpec.LongWave = false;
                surfaceOutputSpec.Temperature = false;

                List<SurfaceOutputSpec> surfaceOutputSpecs = new List<SurfaceOutputSpec>() { surfaceOutputSpec };

                simpleProgressForm.Increment("Simulating");
                analyticalModel = Tas.Modify.RunWorkflow(analyticalModel, path_TBD, null, null, heatingDesignDays, coolingDesignDays, surfaceOutputSpecs, true, simulate, false);

                simpleProgressForm.Increment("Inserting Results");
                List<Core.IResult> results = null;

                AdjacencyCluster adjacencyCluster = null;
                if (analyticalModel != null)
                {
                    adjacencyCluster = analyticalModel?.AdjacencyCluster;
                    if (adjacencyCluster != null)
                    {
                        results = new List<Core.IResult>();
                        adjacencyCluster.GetObjects<SpaceSimulationResult>()?.ForEach(x => results.Add(x));
                        adjacencyCluster.GetObjects<ZoneSimulationResult>()?.ForEach(x => results.Add(x));
                        adjacencyCluster.GetObjects<AdjacencyClusterSimulationResult>()?.ForEach(x => results.Add(x));
                    }
                }

                if (adjacencyCluster != null && results != null && results.Count != 0)
                {
                    ConvertSettings convertSettings = new ConvertSettings(false, true, false);

                    using (Transaction transaction = new Transaction(document, "Simulate"))
                    {
                        transaction.Start();

                        foreach (Core.IResult result in results)
                        {
                            if (result is SpaceSimulationResult)
                            {
                                Convert.ToRevit(adjacencyCluster, (SpaceSimulationResult)result, document, convertSettings)?.Cast<Element>().ToList();
                            }
                            else if (result is ZoneSimulationResult)
                            {
                                Convert.ToRevit(adjacencyCluster, (ZoneSimulationResult)result, document, convertSettings)?.Cast<Element>().ToList();
                            }

                            else if (result is AdjacencyClusterSimulationResult)
                            {
                                Convert.ToRevit((AdjacencyClusterSimulationResult)result, document, convertSettings);
                            }
                        }

                        Modify.CopySpatialElementParameters(document, Tool.TAS);

                        transaction.Commit();
                    }
                }

                simpleProgressForm.Increment("Finishing");
            }

            return Result.Succeeded;
        }

        //public override Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet)
        //{
        //    Document document = externalCommandData?.Application?.ActiveUIDocument?.Document;
        //    if (document == null)
        //    {
        //        return Result.Failed;
        //    }

        //    WeatherData weatherData = null;
        //    using (OpenFileDialog openFileDialog = new OpenFileDialog())
        //    {
        //        openFileDialog.Filter = "epw files (*.epw)|*.epw|TAS TBD files (*.tbd)|*.tbd|TAS TSD files (*.tsd)|*.tsd|TAS TWD files (*.twd)|*.twd|All files (*.*)|*.*";
        //        openFileDialog.FilterIndex = 1;
        //        openFileDialog.RestoreDirectory = true;

        //        if (openFileDialog.ShowDialog() != DialogResult.OK)
        //        {
        //            return Result.Cancelled;
        //        }

        //        string path_WeatherData = openFileDialog.FileName;
        //        string extension = System.IO.Path.GetExtension(path_WeatherData).ToLower().Trim();
        //        if (string.IsNullOrWhiteSpace(extension))
        //        {
        //            return Result.Failed;
        //        }

        //        try
        //        {
        //            if (extension.EndsWith("epw"))
        //            {
        //                weatherData = Weather.Convert.ToSAM(path_WeatherData);
        //            }
        //            else
        //            {
        //                List<WeatherData> weatherDatas = Weather.Tas.Convert.ToSAM_WeatherDatas(path_WeatherData);
        //                if (weatherDatas == null || weatherDatas.Count == 0)
        //                {
        //                    return Result.Failed;
        //                }

        //                if (weatherDatas.Count == 1)
        //                {
        //                    weatherData = weatherDatas[0];
        //                }
        //                else
        //                {
        //                    weatherDatas.Sort((x, y) => x.Name.CompareTo(y.Name));

        //                    using (Core.Windows.Forms.ComboBoxForm<WeatherData> comboBoxForm = new Core.Windows.Forms.ComboBoxForm<WeatherData>("Select Weather Data", weatherDatas, (WeatherData x) => x.Name))
        //                    {
        //                        if (comboBoxForm.ShowDialog() != DialogResult.OK)
        //                        {
        //                            return Result.Cancelled;
        //                        }

        //                        weatherData = comboBoxForm.SelectedItem;
        //                    }

        //                }

        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            weatherData = null;
        //        }
        //    }

        //    if (weatherData == null)
        //    {
        //        return Result.Failed;
        //    }

        //    string path = document.PathName;
        //    if (string.IsNullOrWhiteSpace(path))
        //    {
        //        string name = document.Title;
        //        if (string.IsNullOrWhiteSpace(name))
        //        {
        //            name = "000000_SAM_AnalyticalModel";
        //        }

        //        using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
        //        {
        //            folderBrowserDialog.Description = "Select Directory";
        //            folderBrowserDialog.ShowNewFolderButton = true;
        //            if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
        //            {
        //                return Result.Cancelled;
        //            }

        //            path = System.IO.Path.Combine(folderBrowserDialog.SelectedPath, name + ".rvt");
        //        }

        //        if (string.IsNullOrWhiteSpace(path))
        //        {
        //            return Result.Failed;
        //        }

        //        document.SaveAs(path);
        //    }

        //    AnalyticalModel analyticalModel = null;

        //    using (Transaction transaction = new Transaction(document, "Convert Model"))
        //    {
        //        transaction.Start();
        //        analyticalModel = Convert.ToSAM_AnalyticalModel(document, new ConvertSettings(true, true, false));
        //        transaction.RollBack();
        //    }

        //    if (analyticalModel == null)
        //    {
        //        return Result.Failed;
        //    }

        //    string path_gbXML = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path));
        //    if (System.IO.File.Exists(path_gbXML))
        //    {
        //        System.IO.File.Delete(path_gbXML);
        //    }

        //    bool exported = false;


        //    exported = document.TogbXML(path_gbXML);
        //    if (!exported)
        //    {
        //        return Result.Failed;
        //    }

        //    path_gbXML = path_gbXML + ".xml";

        //    string path_T3D = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".t3d");

        //    exported = Core.Tas.Convert.ToT3D(path_T3D, path_gbXML, true, true, true, false);
        //    if (!exported)
        //    {
        //        return Result.Failed;
        //    }

        //    analyticalModel = Tas.Query.UpdateT3D(analyticalModel, path_T3D);

        //    string path_TBD = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".tbd");

        //    using (SAMTBDDocument sAMTBDDocument = new SAMTBDDocument(path_TBD))
        //    {
        //        TBD.TBDDocument tBDDocument = sAMTBDDocument.TBDDocument;

        //        Weather.Tas.Modify.UpdateWeatherData(tBDDocument, weatherData);

        //        double latitude_TBD = Core.Query.Round(analyticalModel.Location.Latitude, 0.01);
        //        double longitude_TBD = Core.Query.Round(analyticalModel.Location.Longitude, 0.01);

        //        double latitude_WeatherData = Core.Query.Round(weatherData.Latitude, 0.01);
        //        double longitude_WeatherDate = Core.Query.Round(weatherData.Longitude, 0.01);

        //        //if (Math.Abs(latitude_TBD - latitude_WeatherData) > 0.01 || Math.Abs(longitude_TBD - longitude_WeatherDate) > 0.01)
        //        //{
        //        //    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "WeatherData Longitude or Latitude mismatch");
        //        //}

        //        TBD.Calendar calendar = tBDDocument.Building.GetCalendar();

        //        TBD.dayType dayType = null;

        //        dayType = calendar.AddDayType();
        //        dayType.name = "HDD";

        //        dayType = calendar.AddDayType();
        //        dayType.name = "CDD";

        //        sAMTBDDocument.Save();
        //    }

        //    exported = Tas.Convert.ToTBD(path_T3D, path_TBD, 1, 365, 15, true);
        //    if (!exported)
        //    {
        //        return Result.Failed;
        //    }

        //    AdjacencyCluster adjacencyCluster = null;

        //    using (SAMTBDDocument sAMTBDDocument = new SAMTBDDocument(path_TBD))
        //    {
        //        Tas.Query.UpdateFacingExternal(analyticalModel, sAMTBDDocument);

        //        Tas.Modify.AssignAdiabaticConstruction(sAMTBDDocument.TBDDocument.Building, "Adiabatic", new string[] { "-unzoned", "-internal", "-exposed" }, false, true);

        //        Tas.Modify.UpdateBuildingElements(sAMTBDDocument, analyticalModel);

        //        adjacencyCluster = analyticalModel.AdjacencyCluster;
        //        Tas.Modify.UpdateThermalParameters(adjacencyCluster, sAMTBDDocument.TBDDocument?.Building);
        //        analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

        //        Tas.Modify.UpdateZones(analyticalModel, sAMTBDDocument, true);

        //        //Update.DesignDays Missing!!

        //        sAMTBDDocument.Save();
        //    }

        //    Tas.Query.Sizing(path_TBD, analyticalModel, false, true);

        //    analyticalModel = Tas.Modify.UpdateDesignLoads(path_TBD, analyticalModel);

        //    bool hasWeatherData = false;
        //    using (SAMTBDDocument sAMTBDDocument = new SAMTBDDocument(path_TBD))
        //    {
        //        TBD.TBDDocument tBDDocument = sAMTBDDocument.TBDDocument;

        //        hasWeatherData = tBDDocument?.Building.GetWeatherYear() != null;
        //    }

        //    if (!hasWeatherData)
        //    {
        //        MessageBox.Show("Could not complete simulation. TBD file has no Weather Data");
        //        return Result.Failed;
        //    }

        //    string path_TSD = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path) + ".tsd");

        //    bool result = Tas.Modify.Simulate(path_TBD, path_TSD, 1, 365);



        //    adjacencyCluster = analyticalModel.AdjacencyCluster;
        //    Tas.Modify.AddResults(path_TSD, adjacencyCluster);
        //    analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

        //    analyticalModel.ToRevit(document, new ConvertSettings(true, true, false));

        //    return Result.Succeeded;
        //}
    }
}
