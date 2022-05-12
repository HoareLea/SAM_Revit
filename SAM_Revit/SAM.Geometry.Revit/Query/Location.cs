using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static ISAMGeometry3D Location(this Element element)
        {
            if (element == null)
            {
                return null;
            }

            if (element.Location is LocationPoint)
            {
                return ((LocationPoint)element.Location).Point.ToSAM();
            }

            if (element.Location is LocationCurve)
            {
                Curve curve = ((LocationCurve)element.Location).Curve;
                if(curve != null)
                {
                    if (curve is Line)
                    {
                        return ((Line)curve).ToSAM();
                    }

                    return curve.ToSAM_Polyline3D();
                }
            }

            return null;
        }
    }
}