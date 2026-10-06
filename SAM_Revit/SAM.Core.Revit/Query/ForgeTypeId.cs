// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ForgeTypeId ForgeTypeId(this string text)
        {
            if(string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            switch(text)
            {
                case "HVACAirflow":
                    return SpecTypeId.AirFlow;

                case "URL":
                    return SpecTypeId.String.Url;

                case "Text":
                    return SpecTypeId.String.Text;

                case "YesNo":
                    return SpecTypeId.Boolean.YesNo;

                case "Integer":
                    return SpecTypeId.Int.Integer;

                case "Number":
                    return SpecTypeId.Number;

                case "HVACCoefficientOfHeatTransfer":
                    return SpecTypeId.HeatTransferCoefficient;

                case "Volume":
                    return SpecTypeId.Volume;

                case "HVACTemperature":
                    return SpecTypeId.HvacTemperature;

                case "HVACTemperatureDifference":
                    return SpecTypeId.HvacTemperatureDifference;

                case "HVACHeatGain":
                    return SpecTypeId.HeatGain;

                case "HVACCoolingLoad":
                    return SpecTypeId.CoolingLoad;

                case "HVACHeatingLoad":
                    return SpecTypeId.HeatingLoad;

                case "Area":
                    return SpecTypeId.Area;

                case "HVACCoolingLoadDividedByArea":
                    return SpecTypeId.CoolingLoadDividedByArea;

                case "HVACHeatingLoadDividedByArea":
                    return SpecTypeId.HeatingLoadDividedByArea;

                case "HVACCoolingLoadDividedByVolume":
                    return SpecTypeId.CoolingLoadDividedByVolume;

                case "HVACHeatingLoadDividedByVolume":
                    return SpecTypeId.HeatingLoadDividedByVolume;

                case "HVACPower":
                    return SpecTypeId.HvacPower;

                case "HVACPowerDensity":
                    return SpecTypeId.HvacPowerDensity;

                case "HVACPressure":
                    return SpecTypeId.HvacPressure;

                case "HVACDensity":
                    return SpecTypeId.HvacDensity;

                case "HVACVelocity":
                    return SpecTypeId.HvacVelocity;

                case "HVACThermalConductivity":
                    return SpecTypeId.ThermalConductivity;

                case "ElectricalPower":
                    return SpecTypeId.ElectricalPower;

                case "ElectricalIlluminance":
                    return SpecTypeId.Illuminance;

                case "ElectricalEfficacy":
                    return SpecTypeId.Efficacy;

                case "Length":
                    return SpecTypeId.Length;

                case "Angle":
                    return SpecTypeId.Angle;

                case "Material":
                    return SpecTypeId.Reference.Material;
            }

            // An unrecognized spec name returns null rather than throwing. Both callers
            // (AddParameters, GenerateSharedParametersFile) already guard with
            // "if (forgeTypeId != null)" and skip the row, so null is the contract they expect.
            // Throwing aborted the whole command on a single bad cell - one stray value in a
            // ~1000-row spreadsheet meant no parameters were created at all.
            return null;
        }


    }
}