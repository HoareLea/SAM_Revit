using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Line> ToRevit(this IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.MacroDistance)
        {
            return Query.Segment3Ds(closedPlanar3D, tolerance)?.ConvertAll(x => x.ToRevit_Line());
        }
    }
}