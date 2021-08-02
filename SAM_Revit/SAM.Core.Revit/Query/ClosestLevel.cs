using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Level ClosestLevel(this Document document, double elevation)
        {
            if (document == null || double.IsNaN(elevation))
                return null;

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();

            Level result = null;
            double distance = double.MaxValue;
            foreach (Level level in levels)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
double distance_Temp = System.Math.Abs(UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS) - elevation);
#else
                double distance_Temp = System.Math.Abs(UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters) - elevation);
#endif


                if (distance < distance_Temp)
                    continue;

                result = level;
                distance = distance_Temp;
            }

            return result;
        }
    }
}