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

            List<Level> levels = new FilteredElementCollector(level.Document).OfClass(typeof(Level)).Cast<Level>().ToList();

            levels.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            if (level.Elevation < levels.First().Elevation)
                return null;

            for (int i = 1; i < levels.Count; i++)
                if (level.Elevation <= levels[i].Elevation)
                    return levels[i - 1];

            return levels[0];
        }
    }
}