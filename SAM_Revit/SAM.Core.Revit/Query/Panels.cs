using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<Panel> Panels(this Wall wall)
        {
            if (wall == null)
                return null;

            CurtainGrid curtainGrid = wall.CurtainGrid;
            if (curtainGrid == null)
                return null;

            return Panels(wall.Document, curtainGrid);
        }

        public static List<Panel> Panels(this CurtainSystem curtainSystem)
        {
            CurtainGridSet curtainGridSet = curtainSystem?.CurtainGrids;
            if (curtainGridSet == null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();
            foreach (CurtainGrid curtainGrid in curtainGridSet)
            {
                List<Panel> panels = Panels(curtainSystem.Document, curtainGrid);
                if (panels != null && panels.Count != 0)
                {
                    result.AddRange(panels);
                }
            }

            return result;
        }

        public static List<Panel> Panels(this Document document, CurtainGrid curtainGrid)
        {
            if (document == null)
            {
                return null;
            }

            IEnumerable<ElementId> elementIds = curtainGrid?.GetPanelIds();
            if (elementIds == null || elementIds.Count() == 0)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();
            foreach (ElementId elementId in elementIds)
            {
                Panel panel = document.GetElement(elementId) as Panel;
                if (panel == null)
                    continue;

                ElementId elementId_Host = panel.FindHostPanel();
                if (elementId_Host == null || elementId_Host == Autodesk.Revit.DB.ElementId.InvalidElementId)
                {
                    result.Add(panel);
                    continue;
                }

                List<Panel> panels = Panels(document.GetElement(elementId_Host) as Wall);
                if (panels != null && panels.Count > 0)
                    result.AddRange(panels);
            }

            return result;
        }
    }
}