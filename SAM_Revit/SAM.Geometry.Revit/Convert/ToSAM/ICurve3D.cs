using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static ICurve3D ToSAM(this LocationCurve locationCurve)
        {
            Curve curve = locationCurve?.Curve;
            if (curve == null)
                return null;

            return curve.ToSAM();
        }

        public static ICurve3D ToSAM(this Curve curve)
        {
            if (curve == null)
                return null;

            return curve.ToSAM(0.2);
        }

        /// <summary>
        /// Converts Revit Curve to SAM ICurve3D if curve has to be tessellated then it makes sure the length of tessellated segment is greater than minLength
        /// </summary>
        /// <param name="curve">Revit Curve</param>
        /// <param name="minLength"> Min Length if Curve has to be tessellated [m]</param>
        /// <returns>SAM Curve 3D</returns>
        public static ICurve3D ToSAM(this Curve curve, double minLength)
        {
            if (curve == null)
                return null;
            
            if (curve is Line)
                return ((Line)curve).ToSAM();

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (XYZ xyz in curve.Tessellate())
                point3Ds.Add(xyz.ToSAM());

            List<int> indexes = null;
            do
            {
                if(point3Ds.Count > 2)
                {
                    indexes = new List<int>();

                    for (int i = 1; i < point3Ds.Count; i = i + 2)
                    {
                        if (point3Ds[i - 1].Distance(point3Ds[i]) < minLength)
                            indexes.Add(i);
                    }

                    if (indexes != null && indexes.Count > 0)
                    {
                        indexes.Reverse();
                        indexes.ForEach(x => point3Ds.RemoveAt(x));
                    }
                }

            } while (indexes != null && indexes.Count > 0);

            return new Polyline3D(point3Ds);
        }
    }
}