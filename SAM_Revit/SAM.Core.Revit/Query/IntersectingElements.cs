using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<T> IntersectingElements<T>(this Element element, IEnumerable<ElementId> elementIds = null) where T: Element
        {
            if (element == null)
                return null;

            FilteredElementCollector filteredElementCollector = elementIds == null || elementIds.Count() == 0 ? new FilteredElementCollector(element.Document) : new FilteredElementCollector(element.Document, new List<ElementId>(elementIds));

            return filteredElementCollector.OfClass(typeof(T)).WherePasses(new ElementIntersectsElementFilter(element)).ToList().ConvertAll(x => x as T).FindAll(x => x != null);
        }
    }
}