using Autodesk.Revit.DB;

namespace SAM.Units.Revit
{
    public static partial class Query
    {
        public static double Factor_FromFeetToMeter { get; } = UnitUtils.ConvertFromInternalUnits(1, Autodesk.Revit.DB.DisplayUnitType.DUT_METERS);
        public static double Factor_FromMeterToFeet { get; } = UnitUtils.ConvertToInternalUnits(1, Autodesk.Revit.DB.DisplayUnitType.DUT_METERS);
    }
}