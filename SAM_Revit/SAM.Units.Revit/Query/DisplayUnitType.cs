using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Units.Revit
{
    public static partial class Query
    {
        public static DisplayUnitType DisplayUnitType(this UnitType unitType)
        {
            if (unitType == UnitType.UT_Number)
                return Autodesk.Revit.DB.DisplayUnitType.DUT_UNDEFINED;

            IEnumerable<DisplayUnitType> displayUnitTypes = UnitUtils.GetValidDisplayUnits(unitType);
            if (displayUnitTypes == null || displayUnitTypes.Count() == 0)
                return Autodesk.Revit.DB.DisplayUnitType.DUT_UNDEFINED;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_UNDEFINED))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_UNDEFINED;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_METERS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_METERS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_KILOGRAMS_MASS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_KILOGRAMS_MASS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_CELSIUS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_CELSIUS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_AMPERES))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_AMPERES;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_WATTS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_WATTS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_WATTS_PER_CUBIC_METER))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_WATTS_PER_CUBIC_METER;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_METERS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_METERS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_SQUARE_METERS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_SQUARE_METERS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_WATTS_PER_SQUARE_METER))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_WATTS_PER_SQUARE_METER;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_VOLTS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_VOLTS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_KILOGRAMS_FORCE))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_KILOGRAMS_FORCE;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_LUX))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_LUX;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_LUMENS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_LUMENS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_LUMENS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_LUMENS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_PASCALS))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_PASCALS;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_PASCALS_PER_METER))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_PASCALS_PER_METER;

            if (displayUnitTypes.Contains(Autodesk.Revit.DB.DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER))
                return Autodesk.Revit.DB.DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER;

            return Autodesk.Revit.DB.DisplayUnitType.DUT_UNDEFINED;
        }
    }
}