using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Vector3D ToSAM_Vector3D(this XYZ XYZ, bool convertUnits)
        {
            if (convertUnits)

                return new Vector3D(XYZ.X * Units.Revit.ConversionFactor.FromFeetToMeter, XYZ.Y * Units.Revit.ConversionFactor.FromFeetToMeter, XYZ.Z * Units.Revit.ConversionFactor.FromFeetToMeter);
            else
                return new Vector3D(XYZ.X, XYZ.Y, XYZ.Z);
        }
    }
}