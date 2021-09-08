using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Polygon3D> ToSAM_Polygon3Ds(this Autodesk.Revit.DB.Face face, double tolerance = Core.Tolerance.Distance)
        {
            if (face == null)
            {
                return null;
            }

            if(face is PlanarFace)
            {
                return ToSAM_Polygon3Ds((PlanarFace)face, tolerance);
            }

            List<Polygon3D> result = new List<Polygon3D>();

            List<Triangle3D> triangle3Ds = face.Triangulate(0)?.ToSAM(tolerance)?.GetTriangles();
            foreach(Triangle3D triangle3D in triangle3Ds)
            {
                Polygon3D polygon3D = Spatial.Create.Polygon3D(triangle3D);
                if(polygon3D == null)
                {
                    continue;
                }

                result.Add(polygon3D);
            }

            return result;
        }

        public static List<Polygon3D> ToSAM_Polygon3Ds(this PlanarFace planarFace, double tolerance = Core.Tolerance.Distance)
        {
            if(planarFace == null)
            {
                return null;
            }

            XYZ normal = planarFace.FaceNormal;
            if(normal == null)
            {
                return null;
            }

            IEnumerable<CurveLoop> curveLoops = planarFace.GetEdgesAsCurveLoops();
            if (curveLoops == null || curveLoops.Count() == 0)
            {
                return null;
            }

            List<Polygon3D> result = new List<Polygon3D>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = ToSAM_Polygon3D(curveLoop, normal, tolerance);
                if (!Spatial.Query.IsValid(polygon3D))
                {
                    continue;
                }

                List<Polygon3D> polygon3Ds_Intersection = Spatial.Query.SelfIntersectionPolygon3Ds(polygon3D, tolerance);
                if (polygon3Ds_Intersection != null)
                    result.AddRange(polygon3Ds_Intersection);
                else
                    result.Add(polygon3D);
            }

            if (result != null && result.Count > 1)
            {
                Spatial.Plane plane = result[0].GetPlane();
                for (int i = 1; i < result.Count; i++)
                {
                    result[i] = plane.Project(result[i]);
                }
            }

            return result;

        }
    }
}