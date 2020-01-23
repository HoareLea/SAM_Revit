using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Polygon3D ToSAM_Polygon3D(this CurveLoop curveLoop)
        {
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Curve curve in curveLoop)
                point3Ds.Add(curve.ToSAM().GetStart());

            return new Polygon3D(point3Ds);
        }

        public static Polygon3D ToSAM_Polygon3D(this CurveArray curveArray)
        {
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Curve curve in curveArray)
                point3Ds.Add(curve.ToSAM().GetStart());

            return new Polygon3D(point3Ds);
        }
    }
}
