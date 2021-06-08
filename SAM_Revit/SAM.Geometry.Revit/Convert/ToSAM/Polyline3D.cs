using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Polyline3D ToSAM(this IEnumerable<XYZ> xyzs)
        {
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (XYZ xyz in xyzs)
                point3Ds.Add(xyz.ToSAM());

            return new Polyline3D(point3Ds);
        }

        public static Polyline3D ToSAM_Polyline3D(this Curve curve)
        {
            if (curve == null)
            {
                return null;
            }

            List<Point3D> point3Ds = new List<Point3D>();
            if (curve is Line)
            {
                point3Ds.Add(curve.GetEndPoint(0).ToSAM());
                point3Ds.Add(curve.GetEndPoint(1).ToSAM());
            }
            else
            {
                foreach (XYZ xyz in curve.Tessellate())
                    point3Ds.Add(xyz.ToSAM());
            }

            return new Polyline3D(point3Ds);
        }
    }
}