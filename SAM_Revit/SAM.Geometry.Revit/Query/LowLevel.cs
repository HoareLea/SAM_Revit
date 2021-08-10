using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        /// <summary>
        /// Level equal or below given elevation
        /// </summary>
        /// <param name="document">Revit document</param>
        /// <param name="elevation">Elevation in meters [m]</param>
        public static Level LowLevel(this Document document, double elevation)
        {
            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (levels == null || levels.Count == 0)
                return null;

            levels.Sort((x, y) => y.Elevation.CompareTo(x.Elevation));

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
            double levelElevation = UnitUtils.ConvertFromInternalUnits(levels.First().Elevation, DisplayUnitType.DUT_METERS);
#else
            double levelElevation = UnitUtils.ConvertFromInternalUnits(levels.First().Elevation, UnitTypeId.Meters);
#endif

            if (System.Math.Abs(elevation - levelElevation) < Core.Tolerance.MacroDistance)
                return levels.First();

            for (int i = 1; i < levels.Count; i++)
            {

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                levelElevation = UnitUtils.ConvertFromInternalUnits(levels[i].Elevation, DisplayUnitType.DUT_METERS);
#else
                levelElevation = UnitUtils.ConvertFromInternalUnits(levels[i].Elevation, UnitTypeId.Meters);
#endif

                if (System.Math.Round(elevation, 3, MidpointRounding.AwayFromZero) >= System.Math.Round(levelElevation, 3, MidpointRounding.AwayFromZero))
                    return levels[i];
            }

            return levels.Last();
        }
    }
}