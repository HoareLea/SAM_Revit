using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Element> ToRevit(this AdjacencyCluster adjacencyCluster, Document document, ConvertSettings convertSettings)
        {
            if (adjacencyCluster == null || document == null)
                return null;

            List<Element> result = convertSettings?.GetObjects<Element>(adjacencyCluster.Guid);
            if (result != null)
                return result;

            Dictionary<Space, Shell> dictionary_Shell = adjacencyCluster.ShellDictionary();
            if (dictionary_Shell == null)
                return null;

            //List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            //if (levels == null || levels.Count == 0)
            //    return null;

            Dictionary<ElementId, Element> dictionary_Element = new Dictionary<ElementId, Element>();

            HashSet<System.Guid> guids = new HashSet<System.Guid>();
            foreach (KeyValuePair<Space, Shell> keyValuePair in dictionary_Shell)
            {
                Space space = keyValuePair.Key;

                List<Panel> panels_Space = adjacencyCluster.GetPanels(space);
                if (panels_Space != null && panels_Space.Count != 0)
                {
                    foreach (Panel panel in panels_Space)
                    {
                        if (guids.Contains(panel.Guid))
                            continue;

                        guids.Add(panel.Guid);

                        HostObject hostObject = panel.ToRevit(document, convertSettings);
                        if (hostObject == null)
                            continue;

                        dictionary_Element[hostObject.Id] = hostObject;
                    }
                }

                Autodesk.Revit.DB.Mechanical.Space space_Revit = space.ToRevit(document, convertSettings);
                if (space_Revit == null)
                    continue;

                dictionary_Element[space_Revit.Id] = space_Revit;

                BoundingBox3D boundingBox3D = keyValuePair.Value.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                Parameter parameter;

                parameter = space_Revit.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL);
                Level level = document.ClosestLevel(boundingBox3D.Max.Z);
                if (level == null)
                    continue;

                parameter.Set(level.Id);

                if (level.Id != space_Revit.LevelId && level.Elevation > (document.GetElement(space_Revit.LevelId) as Level).Elevation)
                {
                    parameter = space_Revit.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                    parameter.Set(0);
                }
            }

            List<Panel> panels = adjacencyCluster.GetShadingPanels();
            if(panels != null && panels.Count != 0)
            {
                foreach(Panel panel in panels)
                {
                    HostObject hostObject = panel.ToRevit(document, convertSettings);
                    if (hostObject == null)
                        continue;

                    dictionary_Element[hostObject.Id] = hostObject;
                }
            }

            List<Zone> zones = adjacencyCluster.GetObjects<Zone>();
            if (zones != null)
            {
                foreach (Zone zone in zones)
                {
                    List<Autodesk.Revit.DB.Mechanical.Space> spaces = ToRevit(adjacencyCluster, zone, document, convertSettings);
                    spaces?.ForEach(x => dictionary_Element[x.Id] = x);
                }
            }

            List<ZoneSimulationResult> zoneSimulationResults = adjacencyCluster.GetObjects<ZoneSimulationResult>();
            if(zoneSimulationResults != null)
            {
                foreach(ZoneSimulationResult zoneSimulationResult in zoneSimulationResults)
                {
                    List<Autodesk.Revit.DB.Mechanical.Space> spaces = ToRevit(adjacencyCluster, zoneSimulationResult, document, convertSettings);
                    spaces?.ForEach(x => dictionary_Element[x.Id] = x);
                }
            }

            List<SpaceSimulationResult> spaceSimulationResults = adjacencyCluster.GetObjects<SpaceSimulationResult>();
            if (spaceSimulationResults != null)
            {
                foreach (SpaceSimulationResult spaceSimulationResult in spaceSimulationResults)
                {
                    List<Autodesk.Revit.DB.Mechanical.Space> spaces = ToRevit(adjacencyCluster, spaceSimulationResult, document, convertSettings);
                    spaces?.ForEach(x => dictionary_Element[x.Id] = x);
                }
            }

            List<AdjacencyClusterSimulationResult> adjacencyClusterSimulationResults = adjacencyCluster.GetObjects<AdjacencyClusterSimulationResult>();
            if (adjacencyClusterSimulationResults != null)
            {
                foreach (AdjacencyClusterSimulationResult adjacencyClusterSimulationResult in adjacencyClusterSimulationResults)
                {
                    ProjectInfo projectInfo = ToRevit(adjacencyClusterSimulationResult, document, convertSettings);
                    if (projectInfo != null)
                        dictionary_Element[projectInfo.Id] = projectInfo;
                }
            }

            result = dictionary_Element.Values.ToList();

            convertSettings?.Add(adjacencyCluster.Guid, result);

            return result;
        }

        public static List<Element> ToRevit(this AnalyticalModel analyticalModel, Document document, ConvertSettings convertSettings)
        {
            if (analyticalModel == null || document == null)
                return null;

            List<Element> result = convertSettings?.GetObjects<Element>(analyticalModel.Guid);
            if (result != null)
                return result;

            result = ToRevit(analyticalModel.AdjacencyCluster, document, convertSettings);

            if (convertSettings.ConvertParameters)
            {
                ProjectInfo projectInfo = document.ProjectInformation;
                if(projectInfo != null)
                {
                    if (result.Find(x => x.Id == projectInfo.Id) == null)
                        result.Add(projectInfo);

                    Core.Revit.Modify.SetValues(projectInfo, analyticalModel);
                    Core.Revit.Modify.SetValues(projectInfo, analyticalModel, ActiveSetting.Setting, convertSettings.GetParameters());
                }
            }

            convertSettings?.Add(analyticalModel.Guid, result);

            return result;
        }
    }
}