using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        /// <summary>
        /// Level equal or above given elevation
        /// </summary>
        /// <param name="document">Revit document</param>
        /// <param name="elevation">Elevation in meters [m]</param>
        public static Level HighLevel(this Document document, double elevation)
        {
            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (levels == null || levels.Count == 0)
                return null;

            levels.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            double levelElevation = UnitUtils.ConvertFromInternalUnits(levels.First().Elevation, DisplayUnitType.DUT_METERS);

            if (Math.Abs(elevation - levelElevation) < Core.Tolerance.MacroDistance)
                return levels.First();

            for (int i = 1; i < levels.Count; i++)
            {
                levelElevation = UnitUtils.ConvertFromInternalUnits(levels[i].Elevation, DisplayUnitType.DUT_METERS);

                if (Math.Round(elevation, 3, MidpointRounding.AwayFromZero) <= Math.Round(levelElevation, 3, MidpointRounding.AwayFromZero))
                    return levels[i];
            }

            return levels.Last();
        }
    }
}