using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Spatial.Face3D> TopProfiles(this HostObject hostObject)
        {
            List<Spatial.Face3D> result = new List<Spatial.Face3D>();
            foreach (Reference reference in HostObjectUtils.GetTopFaces(hostObject))
            {
                GeometryObject geometryObject = hostObject.GetGeometryObjectFromReference(reference);
                if (geometryObject == null)
                    continue;

                Autodesk.Revit.DB.Face face = geometryObject as Autodesk.Revit.DB.Face;
                if (face == null)
                    continue;

                Spatial.Face3D face3D = face.ToSAM();
                if (face3D == null)
                    continue;

                result.Add(face3D);
            }

            return result;
        }
    }
}