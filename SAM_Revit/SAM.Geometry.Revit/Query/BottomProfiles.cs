using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<IClosed3D> BottomProfiles(this HostObject hostObject)
        {
            List<IClosed3D> result = new List<IClosed3D>();
            foreach (Reference reference in HostObjectUtils.GetBottomFaces(hostObject))
            {
                GeometryObject geometryObject = hostObject.GetGeometryObjectFromReference(reference);
                if (geometryObject == null)
                    continue;

                Autodesk.Revit.DB.Face face = geometryObject as Autodesk.Revit.DB.Face;
                if (face == null)
                    continue;

                result.AddRange(face.ToSAM_PolycurveLoop3Ds());
            }

            return result;
        }
    }
}
