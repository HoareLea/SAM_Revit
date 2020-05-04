using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Line ToRevit(this Segment3D segment3D)
        {
            return Line.CreateBound(segment3D.GetStart().ToRevit(), segment3D.GetEnd().ToRevit());
        }

        public static Line ToRevit_Line(this ICurve3D curve3D)
        {
            return Line.CreateBound(curve3D.GetStart().ToRevit(), curve3D.GetEnd().ToRevit());
        }
    }
}