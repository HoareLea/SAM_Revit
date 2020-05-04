using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Polygon3D ToSAM_Polygon3D(this CurveLoop curveLoop)
        {
            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Curve curve in curveLoop)
            {
                ICurve3D curve3D = curve.ToSAM();
                if (curve3D is ISegmentable3D)
                    ((ISegmentable3D)curve3D).GetSegments().ForEach(x => point3Ds.Add(x.GetStart()));
            }

            if (point3Ds.Count < 3)
                return null;

            return Spatial.Create.Polygon3D(point3Ds);
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