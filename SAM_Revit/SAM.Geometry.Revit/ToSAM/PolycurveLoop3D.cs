using System.Collections.Generic;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static PolycurveLoop3D ToSAM(this CurveLoop curveLoop)
        {
            List<ICurve3D> curves = new List<ICurve3D>();
            foreach (Curve curve_Revit in curveLoop)
                curves.Add(curve_Revit.ToSAM());

            return new PolycurveLoop3D(curves);
        }

        public static PolycurveLoop3D ToSAM(this CurveArray curveArray)
        {
            List<ICurve3D> curves = new List<ICurve3D>();
            foreach (Curve curve_Revit in curveArray)
                curves.Add(curve_Revit.ToSAM());

            return new PolycurveLoop3D(curves);
        }
    }
}
