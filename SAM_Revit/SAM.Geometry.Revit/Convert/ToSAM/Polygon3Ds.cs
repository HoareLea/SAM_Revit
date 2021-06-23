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
                return null;

            IEnumerable<CurveLoop> curveLoops = face.GetEdgesAsCurveLoops();

            if (curveLoops == null || curveLoops.Count() == 0)
                return null;

            XYZ normal = face.ComputeNormal(new UV(0.5, 0.5));

            List<Polygon3D> polygon3Ds = new List<Polygon3D>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = ToSAM_Polygon3D(curveLoop, normal, tolerance);
                if (!Spatial.Query.IsValid(polygon3D))
                {
                    continue;
                }

                List<Polygon3D> polygon3Ds_Intersection = Spatial.Query.SelfIntersectionPolygon3Ds(polygon3D, tolerance);
                if (polygon3Ds_Intersection != null)
                    polygon3Ds.AddRange(polygon3Ds_Intersection);
                else
                    polygon3Ds.Add(polygon3D);
            }

            if (polygon3Ds != null && polygon3Ds.Count > 1)
            {
                Spatial.Plane plane = polygon3Ds[0].GetPlane();
                for (int i = 1; i < polygon3Ds.Count; i++)
                {
                    polygon3Ds[i] = plane.Project(polygon3Ds[i]);
                }
            }

            return polygon3Ds;
        }
    }
}