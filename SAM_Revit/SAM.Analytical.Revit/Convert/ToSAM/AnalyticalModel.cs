using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static AnalyticalModel ToSAM_AnalyticalModel(this Document document, ConvertSettings convertSettings)
        {
            if (document == null)
                return null;

            ProjectInfo projectInfo = document.ProjectInformation;

            AnalyticalModel result = convertSettings?.GetObject<AnalyticalModel>(projectInfo?.Id);
            if (result != null)
                return result;

            Core.Location location = Core.Revit.Query.Location(document);
            Address address = null;
            if (projectInfo != null)
                address = new Address(Guid.NewGuid(), projectInfo.BuildingName, projectInfo.Address, null, null, CountryCode.Undefined);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            EnergyAnalysisDetailModelOptions energyAnalysisDetailModelOptions = new EnergyAnalysisDetailModelOptions();
            energyAnalysisDetailModelOptions.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries;
            energyAnalysisDetailModelOptions.EnergyModelType = EnergyModelType.SpatialElement;
            energyAnalysisDetailModelOptions.ExportMullions = true;
            energyAnalysisDetailModelOptions.IncludeShadingSurfaces = true;
            energyAnalysisDetailModelOptions.SimplifyCurtainSystems = false;

            EnergyDataSettings energyDataSettings = EnergyDataSettings.GetFromDocument(document);
            energyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
            energyDataSettings.ExportDefaults = false;
            energyDataSettings.SliverSpaceTolerance = UnitUtils.ConvertToInternalUnits(0.005, DisplayUnitType.DUT_METERS);
            energyDataSettings.AnalysisType = AnalysisMode.BuildingElements;
            energyDataSettings.EnergyModel = false;

            //Reseting Project Base Point
            IEnumerable<Element> elements = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_ProjectBasePoint);
            foreach (Element element in elements)
            {
                if (element.Pinned)
                    element.Pinned = false;

                Parameter parameter = null;

                parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM);
                if (parameter != null && !parameter.IsReadOnly)
                    parameter.Set(0.0);

                parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM);
                if (parameter != null && !parameter.IsReadOnly)
                    parameter.Set(0.0);

                parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                if (parameter != null && !parameter.IsReadOnly)
                    parameter.Set(0.0);

                parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                if (parameter != null && !parameter.IsReadOnly)
                    parameter.Set(0.0);

                parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);
                if (parameter != null && !parameter.IsReadOnly)
                    parameter.Set(0.0);
            }

            //AnalyticalSpaces
            EnergyAnalysisDetailModel energyAnalysisDetailModel = EnergyAnalysisDetailModel.Create(document, energyAnalysisDetailModelOptions);
            IList<EnergyAnalysisSpace> energyAnalysisSpaces = energyAnalysisDetailModel.GetAnalyticalSpaces();
            Dictionary<string, Tuple<Panel, List<Space>>> dictionary = new Dictionary<string, Tuple<Panel, List<Space>>>();
            foreach (EnergyAnalysisSpace energyAnalysisSpace in energyAnalysisSpaces)
            {
                try
                {
                    if (energyAnalysisSpace.Area <= Core.Tolerance.MacroDistance)
                        continue;

                    Space space = energyAnalysisSpace.ToSAM(convertSettings);
                    if (space == null)
                        continue;

                    Shell shell = Geometry.Revit.Convert.ToSAM(energyAnalysisSpace.GetClosedShell());
                    if (shell == null)
                        continue;

                    adjacencyCluster.AddObject(space);

                    foreach (EnergyAnalysisSurface energyAnalysisSurface in energyAnalysisSpace.GetAnalyticalSurfaces())
                    {
                        Tuple<Panel, List<Space>> tuple;
                        if (!dictionary.TryGetValue(energyAnalysisSurface.SurfaceName, out tuple))
                        {
                            Panel panel = energyAnalysisSurface.ToSAM(convertSettings, shell);
                            if (panel == null)
                                continue;

                            tuple = new Tuple<Panel, List<Space>>(panel, new List<Space>());
                            dictionary[energyAnalysisSurface.SurfaceName] = tuple;
                        }

                        tuple.Item2.Add(space);
                    }
                }
                catch
                {

                }
            }

            #region Additional Checks (WIP)
            //Additional Check for wrongly recoginzed internal panels (WIP)
            List<Tuple<string, Panel, Space, Face3D>> tuples_External = new List<Tuple<string, Panel, Space, Face3D>>();
            foreach (KeyValuePair<string, Tuple<Panel, List<Space>>> keyValuePair in dictionary)
            {
                Tuple<Panel, List<Space>> tuple = keyValuePair.Value;
                if (tuple.Item1 != null && tuple.Item2 != null && tuple.Item2.Count == 1)
                {
                    Face3D face3D = tuple.Item1.GetFace3D();
                    if (face3D != null)
                        tuples_External.Add(new Tuple<string, Panel, Space, Face3D>(keyValuePair.Key, tuple.Item1, tuple.Item2[0], face3D));
                }
            }

            while (tuples_External.Count > 0)
            {
                Tuple<string, Panel, Space, Face3D> tuple = tuples_External[0];
                tuples_External.RemoveAt(0);

                Point3D point3D = tuple.Item4.InternalPoint3D();
                if (point3D == null)
                    continue;

                List<Tuple<string, Panel, Space, Face3D>> tuples_Overlap = tuples_External.FindAll(x => x.Item4.Inside(point3D));
                if (tuples_Overlap.Count != 1)
                    continue;

                tuples_Overlap.Add(tuple);

                tuples_Overlap.Sort((x, y) => y.Item4.GetArea().CompareTo(x.Item4.GetArea()));

                tuple = tuples_Overlap[0];
                Tuple<string, Panel, Space, Face3D> tuple_Overlap = tuples_Overlap[1];

                dictionary[tuple.Item1].Item2.Add(tuple_Overlap.Item3);
                dictionary[tuple_Overlap.Item1] = new Tuple<Panel, List<Space>>(new Panel(dictionary[tuple_Overlap.Item1].Item1, PanelType.Shade), null);
                tuples_External.RemoveAt(tuples_External.IndexOf(tuple_Overlap));
            }
            #endregion

            foreach (Tuple<Panel, List<Space>> tuple in dictionary.Values)
            {
                Panel panel = tuple.Item1;

                adjacencyCluster.AddObject(panel);
                tuple.Item2?.ForEach(x => adjacencyCluster.AddRelation(x, panel));
            }

            //AnalyticalShadingSurfaces
            IList<EnergyAnalysisSurface> analyticalShadingSurfaces = energyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
            foreach (EnergyAnalysisSurface energyAnalysisSurface in analyticalShadingSurfaces)
            {
                try
                {

                    Panel panel = energyAnalysisSurface.ToSAM(convertSettings);
                    if (panel == null)
                        continue;

                    panel = new Panel(panel, PanelType.Shade);

                    adjacencyCluster.AddObject(panel);
                }
                catch
                {

                }

            }

            adjacencyCluster.MapZones();

            IEnumerable<IMaterial> materials = Analytical.Query.Materials(adjacencyCluster, Analytical.ActiveSetting.Setting.GetValue<MaterialLibrary>(AnalyticalSettingParameter.DefaultMaterialLibrary));
            MaterialLibrary materialLibrary = Core.Create.MaterialLibrary("Default Material Library", materials);

            IEnumerable<Profile> profiles = Analytical.Query.Profiles(adjacencyCluster, Analytical.ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary));
            ProfileLibrary profileLibrary = new ProfileLibrary("Default Profile Library", profiles);

            result = new AnalyticalModel(document.Title, null, location, address, adjacencyCluster, materialLibrary, profileLibrary);
            result.UpdateParameterSets(document.ProjectInformation, ActiveSetting.Setting.GetValue<TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

            convertSettings?.Add(projectInfo.Id, result);

            return result;
        }
    }
}