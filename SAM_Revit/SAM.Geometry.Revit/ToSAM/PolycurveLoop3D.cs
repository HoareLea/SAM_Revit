using System.Collections.Generic;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static PolycurveLoop3D ToSAM(this CurveLoop curveLoop)
        {
            Transform transform = Transform.Identity.ScaleBasis(Units.Convert.ToSI(1, Units.UnitType.Feet));

            List<ICurve3D> curves = new List<ICurve3D>();
            foreach (Curve curve_Revit in curveLoop)
                curves.Add(curve_Revit.CreateTransformed(transform).ToSAM());

            return new PolycurveLoop3D(curves);
        }
    }
}
