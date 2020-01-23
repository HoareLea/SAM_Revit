using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Spatial.BoundingBox3D BoundingBox3D(this Element element)
        {
            if (element == null)
                return null;

            BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox(null);
            if (boundingBoxXYZ == null)
                return null;

            return new Spatial.BoundingBox3D(boundingBoxXYZ.Min.ToSAM(), boundingBoxXYZ.Max.ToSAM());
        }
    }
}
