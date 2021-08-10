using Autodesk.Revit.DB;

namespace SAM.Units.Revit
{
    public static partial class Convert
    {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
        public static double ToSI(this double value, UnitType unitType)
        {
            if (unitType == UnitType.UT_Number)
                return value;

            DisplayUnitType displayUnitType = Query.DisplayUnitType(unitType);
            if (displayUnitType == DisplayUnitType.DUT_UNDEFINED)
                return value;

            return UnitUtils.ConvertFromInternalUnits(value, displayUnitType);
        }
#else
        public static double ToSI(this double value, ForgeTypeId specTypeId)
        {
            if (specTypeId == SpecTypeId.Number)
                return value;

            ForgeTypeId unitTypeId = Query.UnitTypeId(specTypeId);
            if (unitTypeId == null)
                return value;

            return UnitUtils.ConvertFromInternalUnits(value, unitTypeId);
        }
#endif
    }
}