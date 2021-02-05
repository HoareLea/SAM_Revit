namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static double OccupiedHours28Percentage(SpaceSimulationResult spaceSimulationResult)
        {
            if (spaceSimulationResult == null)
                return double.NaN;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.OccupiedHours28, out int occupiedHours28))
                return double.NaN;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.OccupiedHours, out int occupiedHours))
                return double.NaN;

            if (occupiedHours == 0)
                return 0;

            return (System.Convert.ToDouble(occupiedHours28) / System.Convert.ToDouble(occupiedHours)) * 100;
        }
    }
}