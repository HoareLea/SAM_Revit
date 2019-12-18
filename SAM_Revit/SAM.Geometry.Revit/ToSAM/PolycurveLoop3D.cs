using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static PolycurveLoop3D ToSAM(this CurveLoop curveLoop, Transform transform = null)
        {
            List<ICurve3D> curves = new List<ICurve3D>();
            foreach (Curve curve_Revit in curveLoop)
            {
                ICurve3D curve_SAM;

                if (transform != null)
                    curve_SAM = curve_Revit.CreateTransformed(transform).ToSAM();
                else
                    curve_SAM = curve_Revit.ToSAM();

                curves.Add(curve_SAM);
            }

            return new PolycurveLoop3D(curves);
        }
    }
}
