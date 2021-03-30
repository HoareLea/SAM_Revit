using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Point3D ToSAM(this XYZ xyz)
        {
            return ToSAM(xyz, true);
        }

        public static Point3D ToSAM(this XYZ xyz, bool convertUnits)
        {
            if (convertUnits)
                return new Point3D(xyz.X * Units.Revit.Query.Factor_FromFeetToMeter, xyz.Y * Units.Revit.Query.Factor_FromFeetToMeter, xyz.Z * Units.Revit.Query.Factor_FromFeetToMeter);

            return new Point3D(xyz.X, xyz.Y, xyz.Z);
        }
    }
}