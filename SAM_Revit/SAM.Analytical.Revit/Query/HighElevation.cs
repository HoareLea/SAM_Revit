namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static double HighElevation(this Panel panel)
        {
            return panel.GetBoundingBox().Max.Z;
        }
    }
}