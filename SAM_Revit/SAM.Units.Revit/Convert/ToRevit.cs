using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Units.Revit
{
    public static partial class Convert
    {
        [Obsolete]
        public static double ToRevit(this double value, ParameterType parameterType)
        {
            if (double.IsNaN(value))
                return value;

            if (parameterType == ParameterType.Invalid)
                return value;

            switch (parameterType)
            {
                case ParameterType.Length:
                    return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_METERS);

                case ParameterType.Weight:
                    return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_KILOGRAMS_MASS);

                case ParameterType.HVACTemperature:
                    return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_KELVIN);

                case ParameterType.PipingTemperature:
                    return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_KELVIN);

                case ParameterType.ElectricalCurrent:
                    return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_AMPERES);

                case ParameterType.HVACCoolingLoad:
                    return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_WATTS);

                case ParameterType.HVACHeatingLoad:
                    return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_WATTS);
            }

            return value;
        }

        public static double ToRevit(this double value, UnitType unitType)
        {
            if (unitType == UnitType.UT_Number)
                return value;

            IEnumerable<DisplayUnitType> displayUnitTypes = UnitUtils.GetValidDisplayUnits(unitType);
            if (displayUnitTypes == null || displayUnitTypes.Count() == 0)
                return value;

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_METERS))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_METERS);

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_KILOGRAMS_MASS))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_KILOGRAMS_MASS);

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_KELVIN))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_KELVIN);

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_AMPERES))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_AMPERES);

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_WATTS))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_WATTS);

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_WATTS_PER_CUBIC_METER))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_WATTS_PER_CUBIC_METER);

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_CUBIC_METERS))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_CUBIC_METERS);

            if (displayUnitTypes.Contains(DisplayUnitType.DUT_SQUARE_METERS))
                return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_SQUARE_METERS);

            return value;
        }
    }
}