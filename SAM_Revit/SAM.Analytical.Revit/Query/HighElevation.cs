namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static double HightElevation(this Panel panel)
        {
            return panel.ToPolycurveLoop().GetBoundingBox().Max.Z;
        }
    }
}
