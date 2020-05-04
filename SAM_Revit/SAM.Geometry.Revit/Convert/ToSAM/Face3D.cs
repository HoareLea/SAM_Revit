using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Face3D ToSAM_Face3D(this CurveArray curveArray, bool flip = false)
        {
            Polygon3D polygon3D = curveArray?.ToSAM_Polygon3D();
            if (polygon3D == null)
                return null;

            if (flip)
                polygon3D.Reverse();

            return new Face3D(polygon3D);
        }

        public static Face3D ToSAM(this Autodesk.Revit.DB.Face face)
        {
            return Face3D.Create(face.ToSAM_Polygon3Ds(), false);
        }
    }
}