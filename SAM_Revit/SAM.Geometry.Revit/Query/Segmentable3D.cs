using SAM.Geometry.Spatial;
using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static ISegmentable3D Segmentable3D(this MEPCurve mEPCurve)
        {
            if (mEPCurve == null)
            {
                return null;
            }

            Curve curve = (mEPCurve.Location as LocationCurve)?.Curve;
            if (curve == null)
            {
                return null;
            }

            if(curve is Line)
            {
                Segment3D result = ((Line)curve).ToSAM();
                return result;
            }

            return curve.ToSAM_Polyline3D();
        }
    }
}