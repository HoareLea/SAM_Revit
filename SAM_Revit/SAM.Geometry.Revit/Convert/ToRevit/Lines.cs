using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Line> ToRevit(this IClosedPlanar3D closedPlanar3D, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Angle = Core.Tolerance.Angle)
        {
            return Query.Segment3Ds(closedPlanar3D, tolerance_Distance, tolerance_Angle)?.ConvertAll(x => x.ToRevit_Line());
        }
    }
}