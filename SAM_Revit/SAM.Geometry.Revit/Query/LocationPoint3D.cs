using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Point3D LocationPoint3D(this Element element)
        {
            if (element == null)
                return null;

            if (element.Location is LocationPoint)
                return ((LocationPoint)element.Location).Point.ToSAM();

            if (element.Location is LocationCurve)
                return ((LocationCurve)element.Location).Curve.GetEndPoint(0).ToSAM();

            return null;
        }
    }
}