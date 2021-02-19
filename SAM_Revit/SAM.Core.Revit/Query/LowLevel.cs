using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Level LowLevel(this Level level)
        {
            if (level == null)
                return null;
            
            return LowLevel(level.Document, level.Elevation);
        }

        public static Level LowLevel(this Document document, double elevation)
        {
            if (document == null || double.IsNaN(elevation))
                return null;

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();

            levels.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            if (elevation < levels.First().Elevation)
                return null;

            for (int i = 1; i < levels.Count; i++)
                if (elevation <= levels[i].Elevation)
                    return levels[i - 1];

            return levels[0];
        }
    }
}