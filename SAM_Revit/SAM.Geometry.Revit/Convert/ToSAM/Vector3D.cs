using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Vector3D ToSAM_Vector3D(this XYZ XYZ, bool convertUnits)
        {
            if (convertUnits)

                return new Vector3D(XYZ.X * Units.Revit.Query.Factor_FromFeetToMeter, XYZ.Y * Units.Revit.Query.Factor_FromFeetToMeter, XYZ.Z * Units.Revit.Query.Factor_FromFeetToMeter);
            else
                return new Vector3D(XYZ.X, XYZ.Y, XYZ.Z);

        }
    }
}
