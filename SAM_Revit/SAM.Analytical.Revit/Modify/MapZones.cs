using Autodesk.Revit.DB;
using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static List<Zone> MapZones(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            TextMap textMap;
            if (!Analytical.ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.ZoneMap, out textMap) || textMap == null)
                return null;

            return Analytical.Modify.MapZones(adjacencyCluster, textMap);
        }
    }
}