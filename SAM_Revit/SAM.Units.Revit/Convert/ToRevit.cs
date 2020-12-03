using Autodesk.Revit.DB;

namespace SAM.Units.Revit
{
    public static partial class Convert
    {
        public static double ToRevit(this double value, UnitType unitType)
        {
            if (unitType == UnitType.UT_Number)
                return value;

            DisplayUnitType displayUnitType = Query.DisplayUnitType(unitType);
            if (displayUnitType == DisplayUnitType.DUT_UNDEFINED)
                return value;

            return UnitUtils.ConvertToInternalUnits(value, displayUnitType);
        }
    }
}