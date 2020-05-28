using Autodesk.Revit.DB;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Polygon3D ToSAM_Polygon3D(this CurveLoop curveLoop, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment3D> segment3Ds = new List<Segment3D>();
            foreach (Curve curve in curveLoop)
            {
                ICurve3D curve3D = curve.ToSAM();
                if (curve3D is ISegmentable3D)
                {
                    List<Segment3D> segment3Ds_Temp = ((ISegmentable3D)curve3D).GetSegments();
                    if (segment3Ds_Temp == null || segment3Ds_Temp.Count == 0)
                        continue;

                    segment3Ds.AddRange(segment3Ds_Temp);
                }
            }

            if (segment3Ds.Count < 3)
                return null;

            int count = segment3Ds.Count;

            segment3Ds.Add(segment3Ds[0]);

            bool oriented = true;
            for (int i = 0; i < count; i++)
                if (segment3Ds[i][1].Distance(segment3Ds[i + 1][0]) > tolerance)
                {
                    oriented = false;
                    break;
                }

            segment3Ds.RemoveAt(count);

            if (oriented)
                return Spatial.Create.Polygon3D(segment3Ds.ConvertAll(x => x.GetStart()));

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Segment3D segment3D in segment3Ds)
            {
                Spatial.Modify.Add(point3Ds, segment3D.GetStart(), tolerance);
                Spatial.Modify.Add(point3Ds, segment3D.GetEnd(), tolerance);
            }

            if (point3Ds == null || point3Ds.Count < 3)
                return null;

            Spatial.Plane plane = Spatial.Create.Plane(point3Ds, tolerance);
            if (plane == null)
                return null;

            List<Planar.Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane.Convert(plane.Project(x)));
            if (segment2Ds == null || segment2Ds.Count < 3)
                return null;

            List<Planar.Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
            {
                //Extra case for situation where segment2Ds does not are not properly sorted
                List<Planar.Point2D> point2Ds = new List<Planar.Point2D>();
                List<Planar.Segment2D> segment2Ds_Temp = new List<Planar.Segment2D>(segment2Ds);
                point2Ds.Add(segment2Ds_Temp[0][0]);
                point2Ds.Add(segment2Ds_Temp[0][1]);
                segment2Ds_Temp.RemoveAt(0);
                while (segment2Ds_Temp.Count > 0)
                {
                    Point2D point2D = point2Ds.Last();
                    segment2Ds_Temp.SortByDistance(point2D);
                    Segment2D segment2D = segment2Ds_Temp[0];
                    if (segment2D[0].Distance(point2D) > segment2D[1].Distance(point2D))
                        point2Ds.Add(segment2D[0]);
                    else
                        point2Ds.Add(segment2D[1]);
                    segment2Ds_Temp.RemoveAt(0);
                }
                return plane.Convert(new Polygon2D(point2Ds));
            }

            if (polygon2Ds.Count > 1)
                polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            return plane.Convert(polygon2Ds[0]);
        }

        public static Polygon3D ToSAM_Polygon3D(this CurveArray curveArray)
        {
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Curve curve in curveArray)
                point3Ds.Add(curve.ToSAM().GetStart());

            return Spatial.Create.Polygon3D(point3Ds);
        }
    }
}