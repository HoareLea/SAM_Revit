using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static List<Autodesk.Revit.DB.Panel> Panels(this Wall wall)
        {
            if (wall == null)
                return null;

            CurtainGrid curtainGrid = wall.CurtainGrid;
            if (curtainGrid == null)
                return null;

            IEnumerable<ElementId> elementIds = curtainGrid.GetPanelIds();
            if (elementIds == null || elementIds.Count() == 0)
                return null;

            List<Autodesk.Revit.DB.Panel> result = new List<Autodesk.Revit.DB.Panel>();
            foreach(ElementId elementId in elementIds)
            {
                Autodesk.Revit.DB.Panel panel = wall.Document.GetElement(elementId) as Autodesk.Revit.DB.Panel;
                
                ElementId elementId_Host = panel.FindHostPanel();
                if (elementId_Host == null || elementId_Host == ElementId.InvalidElementId)
                {
                    result.Add(panel);
                    continue;
                }

                List<Autodesk.Revit.DB.Panel> panels = Panels(wall.Document.GetElement(elementId_Host) as Wall);
                if (panels != null && panels.Count > 0)
                    result.AddRange(panels);
            }

            return result;
        }

    }
}
