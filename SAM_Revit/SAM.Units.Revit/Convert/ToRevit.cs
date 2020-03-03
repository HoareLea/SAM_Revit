using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;


namespace SAM.Units.Revit
{
    public static partial class Convert
    {
        public static double ToRevit(this double value, ParameterType parameterType)
        {
            if (double.IsNaN(value))
                return value;

            if (parameterType == ParameterType.Invalid)
                return value;
            
            switch(parameterType)
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
            }

            return value;
        }
    }
}
