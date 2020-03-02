using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Point3D ToSAM(this XYZ XYZ)
        {
            return new Point3D(XYZ.X * factor_FromFeetToMeter, XYZ.Y * factor_FromFeetToMeter, XYZ.Z * factor_FromFeetToMeter);
        }
    }
}
