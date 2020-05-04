using Autodesk.Revit.DB;

namespace SAM.Units.Revit
{
    public static class Query
    {
        public static double Factor_FromFeetToMeter { get; } = UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_METERS);
        public static double Factor_FromMeterToFeet { get; } = UnitUtils.ConvertToInternalUnits(1, DisplayUnitType.DUT_METERS);
    }
}