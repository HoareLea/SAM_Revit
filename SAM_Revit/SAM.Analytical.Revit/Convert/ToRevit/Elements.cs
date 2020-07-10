using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Element> ToRevit(this AdjacencyCluster adjacencyCluster, Document document, Core.Revit.ConvertSettings convertSettings)
        {
            if (adjacencyCluster == null || document == null)
                return null;

            Dictionary<Space, Shell> dictionary = adjacencyCluster.ShellDictionary();
            if (dictionary == null)
                return null;

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (levels == null || levels.Count == 0)
                return null;

            List<Element> result = new List<Element>();

            HashSet<System.Guid> guids = new HashSet<System.Guid>();
            foreach(KeyValuePair<Space, Shell> keyValuePair in dictionary)
            {
                Space space = keyValuePair.Key;

                List<Panel> panels = adjacencyCluster.GetPanels(space);
                if(panels != null && panels.Count != 0)
                {
                    foreach(Panel panel in panels)
                    {
                        if (guids.Contains(panel.Guid))
                            continue;

                        guids.Add(panel.Guid);
                        
                        HostObject hostObject = panel.ToRevit(document, convertSettings);
                        if (hostObject == null)
                            continue;

                        result.Add(hostObject);
                    }
                }

                Autodesk.Revit.DB.Mechanical.Space space_Revit = space.ToRevit(document, convertSettings);
                if (space_Revit == null)
                    continue;

                result.Add(space_Revit);

                BoundingBox3D boundingBox3D = keyValuePair.Value.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                Parameter parameter;

                parameter = space_Revit.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL);
                Level level = document.ClosestLevel(boundingBox3D.Max.Z);
                if (level == null)
                    continue;

                parameter.Set(level.Id);

                parameter = space_Revit.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET);
                parameter.Set(0);
            }

            return result;
        }
    }
}