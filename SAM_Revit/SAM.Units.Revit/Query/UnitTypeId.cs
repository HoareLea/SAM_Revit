namespace SAM.Units.Revit
{
    public static partial class Query
    {

#if Revit2017 || Revit2018 || Revit2019 || Revit2020

#else
        public static ForgeTypeId UnitTypeId(this ForgeTypeId specTypeId)
        {
            if (specTypeId == SpecTypeId.Number)
                return null;

            IEnumerable<ForgeTypeId> unitTypeIds = UnitUtils.GetValidUnits(specTypeId);
            if (unitTypeIds == null || unitTypeIds.Count() == 0)
                return null;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Meters))
                return Autodesk.Revit.DB.UnitTypeId.Meters;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Kilograms))
                return Autodesk.Revit.DB.UnitTypeId.Kilograms;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Celsius))
                return Autodesk.Revit.DB.UnitTypeId.Celsius;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Amperes))
                return Autodesk.Revit.DB.UnitTypeId.Amperes;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Watts))
                return Autodesk.Revit.DB.UnitTypeId.Watts;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.WattsPerCubicMeter))
                return Autodesk.Revit.DB.UnitTypeId.WattsPerCubicMeter;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.WattsPerMeterKelvin))
                return Autodesk.Revit.DB.UnitTypeId.WattsPerMeterKelvin;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.WattsPerSquareMeterKelvin))
                return Autodesk.Revit.DB.UnitTypeId.WattsPerSquareMeterKelvin;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.CubicMeters))
                return Autodesk.Revit.DB.UnitTypeId.CubicMeters;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.SquareMeters))
                return Autodesk.Revit.DB.UnitTypeId.SquareMeters;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.CubicMetersPerSecond))
                return Autodesk.Revit.DB.UnitTypeId.CubicMetersPerSecond;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.WattsPerSquareMeter))
                return Autodesk.Revit.DB.UnitTypeId.WattsPerSquareMeter;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Volts))
                return Autodesk.Revit.DB.UnitTypeId.Volts;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.KilogramsForce))
                return Autodesk.Revit.DB.UnitTypeId.KilogramsForce;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Lux))
                return Autodesk.Revit.DB.UnitTypeId.Lux;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Lumens))
                return Autodesk.Revit.DB.UnitTypeId.Lumens;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.Pascals))
                return Autodesk.Revit.DB.UnitTypeId.Pascals;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.PascalsPerMeter))
                return Autodesk.Revit.DB.UnitTypeId.PascalsPerMeter;

            if (unitTypeIds.Contains(Autodesk.Revit.DB.UnitTypeId.KilogramsPerCubicMeter))
                return Autodesk.Revit.DB.UnitTypeId.KilogramsPerCubicMeter;

            return null;
        }
#endif

    }
}