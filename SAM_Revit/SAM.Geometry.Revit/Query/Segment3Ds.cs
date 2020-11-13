using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Segment3D> Segment3Ds(this IClosedPlanar3D closedPlanar3D, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Angle = Core.Tolerance.Angle)
        {
            if (closedPlanar3D == null)
                return null;

            if (!(closedPlanar3D is ICurvable3D))
                return null;

            List<ICurve3D> curve3Ds = Spatial.Query.Explode(((ICurvable3D)closedPlanar3D).GetCurves());
            if (curve3Ds == null)
                return null;

            List<Segment3D> segment3Ds = curve3Ds.ConvertAll(x => x as Segment3D);
            if (segment3Ds.Contains(null))
                return null;

            Spatial.Plane plane = closedPlanar3D.GetPlane();
            if (plane == null)
                return null;

            List<Planar.Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane.Convert(x));
            segment2Ds.RemoveAll(x => x.GetLength() <= tolerance_Distance);

            Planar.Query.SimplifyBySAM_Angle(segment2Ds, tolerance_Distance, tolerance_Angle);
            Planar.Modify.Snap(segment2Ds);

            return segment2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}