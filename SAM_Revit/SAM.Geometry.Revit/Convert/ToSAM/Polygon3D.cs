using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Polygon3D ToSAM_Polygon3D(this CurveLoop curveLoop, XYZ normal = null, double tolerance = Core.Tolerance.Distance)
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

            Vector3D vector3D = normal?.ToSAM_Vector3D(false);

            if (oriented)
            {
                if(vector3D != null)
                {
                    return Spatial.Create.Polygon3D(vector3D, segment3Ds.ConvertAll(x => x.GetStart()));
                }
                else
                {
                    return Spatial.Create.Polygon3D(segment3Ds.ConvertAll(x => x.GetStart()), Core.Tolerance.MacroDistance);
                }
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Segment3D segment3D in segment3Ds)
            {
                Spatial.Modify.Add(point3Ds, segment3D.GetStart(), tolerance);
                Spatial.Modify.Add(point3Ds, segment3D.GetEnd(), tolerance);
            }

            if (point3Ds == null || point3Ds.Count < 3)
                return null;

            Spatial.Plane plane = null;
            if (vector3D != null)
            {
                plane = new Spatial.Plane(point3Ds.Average(), vector3D);
            }
            else
            {
                plane = Spatial.Create.Plane(point3Ds, Core.Tolerance.MacroDistance);
            }

            if (plane == null)
                return null;

            List<Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane.Convert(plane.Project(x)));
            if (segment2Ds == null || segment2Ds.Count < 3)
                return null;

            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
            {
                //Extra case for situation where segment2Ds does not are not properly sorted
                List<Point2D> point2Ds = new List<Point2D>();
                List<Segment2D> segment2Ds_Temp = new List<Segment2D>(segment2Ds);
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

        public static Polygon3D ToSAM_Polygon3D(this CurveArray curveArray, XYZ normal = null)
        {
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Curve curve in curveArray)
            {
                ISegmentable3D segmentable3D = curve.ToSAM(0.2) as ISegmentable3D;
                if(segmentable3D == null)
                {
                    continue;
                }

                List<Point3D> point3Ds_Temp = segmentable3D.GetPoints();
                if(point3Ds_Temp == null || point3Ds_Temp.Count == 0)
                {
                    continue;
                }

                if(point3Ds_Temp.Count == 1)
                {
                    point3Ds.Add(point3Ds_Temp[0]);
                    continue;
                }

                point3Ds_Temp.RemoveAt(point3Ds_Temp.Count - 1);

                point3Ds_Temp.ForEach(x => point3Ds.Add(x));
            }

            if(point3Ds == null || point3Ds.Count == 0)
            {
                return null;
            }

            Polygon3D result = null;

            if(normal != null)
            {
                result = Spatial.Create.Polygon3D(normal.ToSAM_Vector3D(false), point3Ds);
            }

            if(result == null)
            {
                result =Spatial.Create.Polygon3D(point3Ds);
            }

            return result;
        }

        public static Polygon3D ToSAM(this Polyloop polyloop)
        {
            if (polyloop == null)
                return null;

            List<Point3D> point3Ds = new List<Point3D>();
            foreach(XYZ xyz in polyloop.GetPoints())
                point3Ds.Add(xyz.ToSAM());

            if (point3Ds.Count < 3)
                return null;

            return new Polygon3D(point3Ds);
        }
    }
}