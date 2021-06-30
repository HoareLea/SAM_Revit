using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<ISegmentable3D> ExtremeSegmentable3Ds(this IEnumerable<ISegmentable3D> segmentable3Ds, IEnumerable<ISegmentable3D> segmentable3Ds_Auxiliary, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable3Ds == null || segmentable3Ds_Auxiliary == null)
            {
                return null;
            }

            List<ISegmentable3D> result = new List<ISegmentable3D>();

            Dictionary<ISegmentable3D, List<Vector3D>> dictionary = new Dictionary<ISegmentable3D, List<Vector3D>>();
            foreach (ISegmentable3D segmentable3D in segmentable3Ds)
            {
                ICurve3D curve3D = segmentable3D as ICurve3D;
                if (curve3D == null)
                {
                    continue;
                }

                foreach(Point3D point3D in new Point3D[] { curve3D.GetStart(), curve3D.GetEnd()})
                {
                    List<Point3D> point3Ds_Intersection = segmentable3D.Intersections(segmentable3Ds_Auxiliary, tolerance);
                    Point3D point3D_Closest = point3Ds_Intersection.Closest(point3D);

                    ISegmentable3D segmentable3D_Closest = null;
                    foreach (ISegmentable3D segmentable3D_Temp in segmentable3Ds_Auxiliary)
                    {
                        if (segmentable3D_Temp.On(point3D_Closest, tolerance))
                        {
                            segmentable3D_Closest = segmentable3D_Temp;
                            break;
                        }
                    }

                    if (segmentable3D_Closest != null)
                    {
                        if (!dictionary.TryGetValue(segmentable3D_Closest, out List<Vector3D> vector3Ds))
                        {
                            vector3Ds = new List<Vector3D>();
                            dictionary[segmentable3D_Closest] = vector3Ds;
                        }

                        Vector3D vector3D = new Vector3D(point3D_Closest, point3D);

                        if (vector3Ds.Find(x => x.AlmostEqual(vector3D, tolerance)) == null)
                        {
                            vector3Ds.Add(vector3D);

                            ISegmentable3D segmentable3D_Moved = segmentable3D_Closest.GetMoved(vector3D) as ISegmentable3D;

                            result.Add(segmentable3D_Moved);
                        }
                    }
                }

            }

            return result;
        }
    }
}