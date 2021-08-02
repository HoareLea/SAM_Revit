using Autodesk.Revit.DB;

namespace SAM.Architectural.Revit
{
    public static partial class Query
    {
        public static double Elevation(this Autodesk.Revit.DB.Level level)
        {
            if (level == null)
                return double.NaN;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
            return UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
#else
            return UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters);
#endif
        }
    }
}