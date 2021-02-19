using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Level HighLevel(this Level level)
        {
            if (level == null)
                return null;

            return HighLevel(level.Document, level.Elevation);
        }

        public static Level HighLevel(this Document document, double elevation)
        {
            if (document == null || double.IsNaN(elevation))
                return null;

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();

            levels.Sort((x, y) => y.Elevation.CompareTo(x.Elevation));

            if (elevation > levels.First().Elevation)
                return null;

            for (int i = 1; i < levels.Count; i++)
            {
                if (elevation >= levels[i].Elevation)
                {
                    if (i == 0)
                        return null;

                    return levels[i - 1];
                }
            }

            return levels[0];
        }
    }
}