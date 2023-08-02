using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static XYZ ToRevit(this Point3D point3D)
        {
            return ToRevit(point3D, true);
        }

        public static XYZ ToRevit(this Point3D point3D, bool convertUnits)
        {
            if (convertUnits)
                return new XYZ(point3D.X * Units.Revit.ConversionFactor.FromMeterToFeet, point3D.Y * Units.Revit.ConversionFactor.FromMeterToFeet, point3D.Z * Units.Revit.ConversionFactor.FromMeterToFeet);

            return new XYZ(point3D.X, point3D.Y, point3D.Z);
        }

        public static XYZ ToRevit(this Vector3D vector3D, bool convertUnits)
        {
            if (convertUnits)
                return new XYZ(vector3D.X * Units.Revit.ConversionFactor.FromMeterToFeet, vector3D.Y * Units.Revit.ConversionFactor.FromMeterToFeet, vector3D.Z * Units.Revit.ConversionFactor.FromMeterToFeet);
            else
                return new XYZ(vector3D.X, vector3D.Y, vector3D.Z);
        }
    }
}