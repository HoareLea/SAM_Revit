using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static double LowElevation(this Panel panel)
        {
            return panel.GetBoundingBox().Min.Z;
        }

        public static double LowElevation(this Space space)
        {
            if (space.Location != null)
                return space.Location.Z;

            return double.NaN;
        }

        public static double LowElevation(IEnumerable<Panel> panels)
        {
            if (panels == null || panels.Count() == 0)
                return double.NaN;

            double result = double.MaxValue;
            foreach (Panel panel in panels)
            {
                double value = panel.GetBoundingBox().Min.Z;
                if (value < result)
                    result = value;
            }

            if (result == double.MaxValue)
                result = double.NaN;

            return result;
        }
    }
}