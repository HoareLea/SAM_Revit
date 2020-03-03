using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Point3D ToSAM(this XYZ XYZ)
        {
            return new Point3D(XYZ.X * Units.Revit.Query.Factor_FromFeetToMeter, XYZ.Y * Units.Revit.Query.Factor_FromFeetToMeter, XYZ.Z * Units.Revit.Query.Factor_FromFeetToMeter);
        }
    }
}
