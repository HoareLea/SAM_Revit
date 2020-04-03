using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Level HighLevel(this Level level)
        {
            if (level == null)
                return null;

            List<Level> levels = new FilteredElementCollector(level.Document).OfClass(typeof(Level)).Cast<Level>().ToList();

            levels.Sort((x, y) => y.Elevation.CompareTo(x.Elevation));

            if (level.Elevation > levels.First().Elevation)
                return null;

            for (int i = 1; i < levels.Count; i++)
            {
                if(level.Elevation >= levels[i].Elevation)
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
