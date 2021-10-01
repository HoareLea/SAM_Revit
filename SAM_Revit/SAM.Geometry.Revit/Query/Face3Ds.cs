using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Face3D> Face3Ds(this HostObject hostObject, ElementId generatingElementId)
        {
            List<Autodesk.Revit.DB.Face> faces = Faces(hostObject, generatingElementId);
            if(faces == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();
            foreach(Autodesk.Revit.DB.Face face in faces)
            {
                List<Face3D> face3Ds = face?.ToSAM();
                if(face3Ds == null)
                {
                    continue;
                }

                result.AddRange(face3Ds);
            }

            return result;
        }
    }
}