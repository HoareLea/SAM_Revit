using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Point3D ToSAM(this XYZ XYZ)
        {
            double scale = Units.Query.ToSI(1, Units.UnitType.Feet);

            return new Point3D(XYZ.X * scale, XYZ.Y * scale, XYZ.Z * scale);
        }
    }
}
