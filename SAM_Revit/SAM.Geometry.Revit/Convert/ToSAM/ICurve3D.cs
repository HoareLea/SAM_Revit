using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static ICurve3D ToSAM(this LocationCurve locationCurve)
        {
            return locationCurve.Curve.ToSAM();
        }

        public static ICurve3D ToSAM(this Curve curve)
        {
            // Curve curve_Transformed =
            // curve.CreateTransformed(Transform.Identity.ScaleBasis(Units.Convert.ToSI(1, Units.UnitType.Feet)));

            if (curve is Line)
                return ((Line)curve).ToSAM();
            else
                return curve.Tessellate().ToSAM(); //TODO: Implement proper curve translation
        }
    }
}