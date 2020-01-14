using System;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static double LowElevation(this Panel panel)
        {
            return panel.ToPolycurveLoop().GetBoundingBox().Max.Z;
        }
    }
}
