namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static double LowElevation(this Panel panel)
        {
            return panel.ToPolycurveLoop().GetBoundingBox().Min.Z;
        }
    }
}
