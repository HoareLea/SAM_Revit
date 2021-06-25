using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        /// <summary>
        /// Converts Revit LocationCurve to SAM ICurve3D
        /// </summary>
        /// <param name="locationCurve">Revit Curve</param>
        /// <returns>SAM Curve 3D</returns>
        public static ICurve3D ToSAM(this LocationCurve locationCurve)
        {
            Curve curve = locationCurve?.Curve;
            if (curve == null)
                return null;

            return curve.ToSAM();
        }

        /// <summary>
        /// Converts Revit Curve to SAM ICurve3D
        /// </summary>
        /// <param name="curve">Revit Curve</param>
        /// <returns>SAM Curve 3D</returns>
        public static ICurve3D ToSAM(this Curve curve)
        {
            if (curve == null)
                return null;
            
            if (curve is Line)
                return ((Line)curve).ToSAM();

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (XYZ xyz in curve.Tessellate())
                point3Ds.Add(xyz.ToSAM());

            return new Polyline3D(point3Ds);
        }
    }
}