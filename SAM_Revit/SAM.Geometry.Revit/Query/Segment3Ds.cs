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

            Plane plane = closedPlanar3D.GetPlane();
            if (plane == null)
                return null;

            List<Planar.Point2D> point2Ds = new List<Planar.Point2D>();
            foreach(ICurve3D curve3D in curve3Ds)
            {
                if(curve3D == null)
                {
                    continue;
                }

                Planar.Modify.Add(point2Ds, plane.Convert(curve3D.GetStart()), tolerance_Distance);
                Planar.Modify.Add(point2Ds, plane.Convert(curve3D.GetEnd()), tolerance_Distance);
            }

            point2Ds = Planar.Query.SimplifyByAngle(point2Ds, true, tolerance_Angle);

            return Planar.Create.Segment2Ds(point2Ds, true)?.ConvertAll(x => plane.Convert(x));
        }
    }
}