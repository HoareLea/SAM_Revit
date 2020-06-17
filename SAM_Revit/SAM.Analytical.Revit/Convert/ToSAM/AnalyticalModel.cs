using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static AnalyticalModel ToSAM_AnalyticalModel(this Document document)
        {
            if (document == null)
                return null;

            Core.Location location = Core.Revit.Query.Location(document);
            Core.Address address = null;
            ProjectInfo projectInfo = new FilteredElementCollector(document).OfClass(typeof(ProjectInfo)).FirstOrDefault() as ProjectInfo;
            if (projectInfo != null)
                address = new Core.Address(Guid.NewGuid(), projectInfo.BuildingName, projectInfo.Address, null, null, Core.CountryCode.Undefined);
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

                    Space space = energyAnalysisSpace.ToSAM();
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
                            Panel panel = energyAnalysisSurface.ToSAM(shell);
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

            foreach (Tuple<Panel, List<Space>> tuple in dictionary.Values)
            {
                Panel panel = tuple.Item1;

                adjacencyCluster.AddObject(panel);
                tuple.Item2.ForEach(x => adjacencyCluster.AddRelation(x, panel));
            }

            //AnalyticalShadingSurfaces
            IList<EnergyAnalysisSurface> analyticalShadingSurfaces = energyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
            foreach (EnergyAnalysisSurface energyAnalysisSurface in analyticalShadingSurfaces)
            {
                try
                {

                    Panel panel = energyAnalysisSurface.ToSAM();
                    if (panel == null)
                        continue;

                    panel = new Panel(panel, PanelType.Shade);

                    adjacencyCluster.AddObject(panel);
                }
                catch
                {

                }

            }


            return new AnalyticalModel(document.Title, null, location, address, adjacencyCluster);
        }
    }
}