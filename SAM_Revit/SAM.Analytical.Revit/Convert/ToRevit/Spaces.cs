using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Autodesk.Revit.DB.Mechanical.Space> ToRevit(this AdjacencyCluster adjacencyCluster, Zone zone, Document document, ConvertSettings convertSettings)
        {
            if (adjacencyCluster == null || zone == null)
                return null;

            List<Autodesk.Revit.DB.Mechanical.Space> result = convertSettings?.GetObjects<Autodesk.Revit.DB.Mechanical.Space>(zone.Guid);
            if (result != null)
                return result;

            Zone zone_Temp = adjacencyCluster.GetObject<Zone>(zone.Guid);
            if (zone_Temp == null)
                zone_Temp = zone;

            if (convertSettings.ConvertParameters)
            {
                string zoneParameterName = Query.ZoneParameterName(zone);

                ZoneType zoneType = zone.ZoneType();
               
                List<Space> spaces = adjacencyCluster.GetSpaces(zone_Temp);
                if (spaces != null)
                {
                    List<Autodesk.Revit.DB.Mechanical.Space> spaces_Revit = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();

                    foreach (Space space in spaces)
                    {
                        Autodesk.Revit.DB.Mechanical.Space space_Revit = Core.Revit.Query.Element<Autodesk.Revit.DB.Mechanical.Space>(document, space);
                        if (space_Revit == null)
                        {
                            string name = space.Name;
                            if (name != null)
                                space_Revit = spaces_Revit?.Find(x => x.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() == name);
                        }

                        if (space_Revit == null)
                            continue;

                        if (!string.IsNullOrWhiteSpace(zoneParameterName))
                        {
                            IEnumerable<Parameter> parameters = space_Revit.GetParameters(zoneParameterName);
                            if(parameters != null)
                            {
                                foreach (Parameter parameter in parameters)
                                {
                                    if (parameter == null || parameter.IsReadOnly || parameter.StorageType != StorageType.String)
                                        continue;

                                    parameter.Set(zone.Name);
                                }
                            }
                        }

                        Core.Revit.Modify.SetValues(space_Revit, zone_Temp);
                        Core.Revit.Modify.SetValues(space_Revit, zone_Temp, ActiveSetting.Setting);

                        if(zoneType != ZoneType.Undefined)
                            Modify.SetValues(space_Revit, zone_Temp, ActiveSetting.Setting, zoneType, convertSettings?.GetParameters());
                    }
                }
            }

            convertSettings?.Add(zone.Guid, result);

            return result;
        }

        public static List<Autodesk.Revit.DB.Mechanical.Space> ToRevit(this AdjacencyCluster adjacencyCluster, ZoneSimulationResult zoneSimulationResult, Document document, ConvertSettings convertSettings)
        {
            if (adjacencyCluster == null || zoneSimulationResult == null)
                return null;

            List<Autodesk.Revit.DB.Mechanical.Space> result = convertSettings?.GetObjects<Autodesk.Revit.DB.Mechanical.Space>(zoneSimulationResult.Guid);
            if (result != null)
                return result;

            ZoneSimulationResult zoneSimulationResult_Temp = adjacencyCluster.GetObject<ZoneSimulationResult>(zoneSimulationResult.Guid);
            if (zoneSimulationResult_Temp == null)
                zoneSimulationResult_Temp = zoneSimulationResult;

            List<Zone> zones = adjacencyCluster.GetRelatedObjects<Zone>(zoneSimulationResult_Temp);
            if(zones != null && zones.Count != 0)
            {
                if (convertSettings.ConvertParameters)
                {
                    foreach(Zone zone in zones)
                    {
                        List<Space> spaces = adjacencyCluster.GetSpaces(zone);
                        if (spaces != null)
                        {
                            ZoneType zoneType = zone.ZoneType();
                            if (zoneType == ZoneType.Undefined)
                                continue;

                            List<Autodesk.Revit.DB.Mechanical.Space> spaces_Revit = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();

                            foreach (Space space in spaces)
                            {
                                Autodesk.Revit.DB.Mechanical.Space space_Revit = Core.Revit.Query.Element<Autodesk.Revit.DB.Mechanical.Space>(document, space);
                                if (space_Revit == null)
                                {
                                    string name = space.Name;
                                    if (name != null)
                                        space_Revit = spaces_Revit?.Find(x => x.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() == name);
                                }

                                if (space_Revit == null)
                                    continue;

                                Core.Revit.Modify.SetValues(space_Revit, zoneSimulationResult);
                                Core.Revit.Modify.SetValues(space_Revit, zoneSimulationResult, ActiveSetting.Setting);

                                Modify.SetValues(space_Revit, zoneSimulationResult, ActiveSetting.Setting, zoneType, convertSettings?.GetParameters());
                            }
                        }
                    }
                }
            }

            convertSettings?.Add(zoneSimulationResult.Guid, result);

            return result;
        }

        public static List<Autodesk.Revit.DB.Mechanical.Space> ToRevit(this AdjacencyCluster adjacencyCluster, SpaceSimulationResult spaceSimulationResult, Document document, ConvertSettings convertSettings)
        {
            if (spaceSimulationResult == null)
                return null;

            List<Autodesk.Revit.DB.Mechanical.Space> result = convertSettings?.GetObjects<Autodesk.Revit.DB.Mechanical.Space>(spaceSimulationResult.Guid);
            if (result != null)
                return result;

            List<Autodesk.Revit.DB.Mechanical.Space> spaces_Revit = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();

            if (adjacencyCluster != null)
            {
                List<Space> spaces_SAM = adjacencyCluster.GetRelatedObjects<Space>(spaceSimulationResult);
                if(spaces_SAM != null)
                {
                    foreach(Space space_SAM in spaces_SAM)
                    {
                        Autodesk.Revit.DB.Mechanical.Space space_Revit = Core.Revit.Query.Element<Autodesk.Revit.DB.Mechanical.Space>(document, spaceSimulationResult);
                        if(space_Revit == null)
                            space_Revit = spaces_Revit?.Find(x => x.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() == space_SAM.Name);

                        if (space_Revit == null)
                            continue;

                        if (result == null)
                            result = new List<Autodesk.Revit.DB.Mechanical.Space>();

                        result.Add(space_Revit);
                    }
                }
            }

            if(result == null)
            {
                Autodesk.Revit.DB.Mechanical.Space space_Revit = Core.Revit.Query.Element<Autodesk.Revit.DB.Mechanical.Space>(document, spaceSimulationResult);
                if (space_Revit == null)
                    space_Revit = spaces_Revit?.Find(x => x.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() == spaceSimulationResult.Name);

                if (space_Revit != null)
                    result = new List<Autodesk.Revit.DB.Mechanical.Space>() { space_Revit };
            }

            if (result == null)
                return result;

            if (convertSettings.ConvertParameters)
            {
                foreach(Autodesk.Revit.DB.Mechanical.Space space in result)
                {
                    Core.Revit.Modify.SetValues(space, spaceSimulationResult);
                    Core.Revit.Modify.SetValues(space, spaceSimulationResult, ActiveSetting.Setting);
                    Modify.SetValues(space, spaceSimulationResult, ActiveSetting.Setting, spaceSimulationResult.LoadType(), convertSettings?.GetParameters());
                }
            }

            convertSettings?.Add(spaceSimulationResult.Guid, result);

            return result;
        }
    }
}