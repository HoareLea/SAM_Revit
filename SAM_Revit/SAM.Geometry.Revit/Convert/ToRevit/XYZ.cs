using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        private static double factor_FromFeetToMeter = UnitUtils.ConvertFromInternalUnits(1, DisplayUnitType.DUT_METERS);
        private static double factor_FromMeterToFeet = UnitUtils.ConvertToInternalUnits(1, DisplayUnitType.DUT_METERS);

        public static XYZ ToRevit(this Point3D point3D)
        {
            return new XYZ(point3D.X * factor_FromMeterToFeet, point3D.Y * factor_FromMeterToFeet, point3D.Z * factor_FromMeterToFeet);
        }

        public static XYZ ToRevit(this Vector3D vector3D)
        {
            return new XYZ(vector3D.X * factor_FromMeterToFeet, vector3D.Y * factor_FromMeterToFeet, vector3D.Z * factor_FromMeterToFeet);
        }
    }
}
