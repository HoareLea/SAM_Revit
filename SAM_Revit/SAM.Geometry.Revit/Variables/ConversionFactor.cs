using Autodesk.Revit.DB;

namespace SAM.Units.Revit
{
    public static class ConversionFactor
    {

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
        public static double FromFeetToMeter { get; } = UnitUtils.ConvertFromInternalUnits(1, Autodesk.Revit.DB.DisplayUnitType.DUT_METERS);
        public static double FromMeterToFeet { get; } = UnitUtils.ConvertToInternalUnits(1, Autodesk.Revit.DB.DisplayUnitType.DUT_METERS);
#else
        public static double FromFeetToMeter { get; } = UnitUtils.ConvertFromInternalUnits(1, Autodesk.Revit.DB.UnitTypeId.Meters);
        public static double FromMeterToFeet { get; } = UnitUtils.ConvertToInternalUnits(1, Autodesk.Revit.DB.UnitTypeId.Meters);
#endif
    }
}