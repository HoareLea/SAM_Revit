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
    }
}