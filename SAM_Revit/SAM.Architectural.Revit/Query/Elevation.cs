using Autodesk.Revit.DB;

namespace SAM.Architectural.Revit
{
    public static partial class Query
    {
        public static double Elevation(this Autodesk.Revit.DB.Level level)
        {
            if (level == null)
                return double.NaN;

            return UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
        }
    }
}