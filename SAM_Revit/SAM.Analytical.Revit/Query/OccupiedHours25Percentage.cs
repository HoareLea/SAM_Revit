namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static double OccupiedHours25Percentage(SpaceSimulationResult spaceSimulationResult)
        {
            if (spaceSimulationResult == null)
                return double.NaN;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.OccupiedHours25, out int occupiedHours25))
                return double.NaN;

            if (!spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.OccupiedHours, out int occupiedHours))
                return double.NaN;

            if (occupiedHours == 0)
                return 0;

            return (System.Convert.ToDouble(occupiedHours25) / System.Convert.ToDouble(occupiedHours)) * 100;
        }
    }
}