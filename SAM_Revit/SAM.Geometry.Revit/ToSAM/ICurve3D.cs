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
        public static ICurve3D ToSAM(this LocationCurve locationCurve)
        {
            return locationCurve.Curve.ToSAM();
        }

        public static ICurve3D ToSAM(this Curve curve)
        {
            if (curve is Line)
                return ((Line)curve).ToSAM();
            else
                return curve.Tessellate().ToSAM(); //TODO: Implement proper curve translation
        }
    }
}
