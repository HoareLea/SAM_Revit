using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Polygon3D> ToSAM_Polygon3Ds(this Autodesk.Revit.DB.Face face, XYZ normal = null)
        {
            if (face == null)
                return null;

            return ToSAM_Polygon3Ds(face.GetEdgesAsCurveLoops(), normal);
        }

        public static List<Polygon3D> ToSAM_Polygon3Ds(this IEnumerable<CurveLoop> curveLoops, XYZ normal = null, double tolerance = Core.Tolerance.Distance)
        {
            if (curveLoops == null || curveLoops.Count() == 0)
                return null;

            List<Polygon3D> polygon3Ds = new List<Polygon3D>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = ToSAM_Polygon3D(curveLoop, normal, tolerance);
                if(!Spatial.Query.IsValid(polygon3D))
                {
                    continue;
                }

                List<Polygon3D> polygon3Ds_Intersection = Spatial.Query.SelfIntersectionPolygon3Ds(polygon3D, tolerance);
                if (polygon3Ds_Intersection != null)
                    polygon3Ds.AddRange(polygon3Ds_Intersection);
                else
                    polygon3Ds.Add(polygon3D);
            }

            return polygon3Ds;
        }
    }
}