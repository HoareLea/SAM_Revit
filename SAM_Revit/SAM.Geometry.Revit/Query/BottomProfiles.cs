using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Spatial.Face3D> BottomProfiles(this HostObject hostObject)
        {
            List<Spatial.Face3D> result = new List<Spatial.Face3D>();
            foreach (Reference reference in HostObjectUtils.GetBottomFaces(hostObject))
            {
                GeometryObject geometryObject = hostObject.GetGeometryObjectFromReference(reference);
                if (geometryObject == null)
                    continue;

                Autodesk.Revit.DB.Face face = geometryObject as Autodesk.Revit.DB.Face;
                if (face == null)
                    continue;

                result.AddRange(face.ToSAM_Faces());
            }

            return result;
        }
    }
}
