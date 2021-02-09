using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string ZoneId(this Zone zone)
        {
            if (zone == null)
                return null;

            switch(zone.ZoneType())
            {
                case ZoneType.Cooling:
                    return "CLG";
                case ZoneType.Heating:
                    return "HTG";
                case ZoneType.Ventilation:
                    return "VNT";
            }

            return null;
        }

        public static string ZoneId(this AdjacencyCluster adjacencyCluster, ZoneSimulationResult zoneSimulationResult)
        {
            if (adjacencyCluster == null || zoneSimulationResult == null)
                return null;

            Zone zone = adjacencyCluster.GetRelatedObjects<Zone>(zoneSimulationResult).FirstOrDefault();
            if (zone == null)
                return null;

            return ZoneId(zone);
        }
    }
}