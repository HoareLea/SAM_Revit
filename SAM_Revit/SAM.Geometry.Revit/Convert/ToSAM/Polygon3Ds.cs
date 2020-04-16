using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using SAM.Core;
using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Polygon3D> ToSAM_Polygon3Ds(this Autodesk.Revit.DB.Face face)
        {
            if (face == null)
                return null;
            
            return ToSAM_Polygon3Ds(face.GetEdgesAsCurveLoops());
        }

        public static List<Polygon3D> ToSAM_Polygon3Ds(this IEnumerable<CurveLoop> curveLoops)
        {
            if (curveLoops == null || curveLoops.Count() == 0)
                return null;
            
            List<Polygon3D> polygon3Ds = new List<Polygon3D>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = ToSAM_Polygon3D(curveLoop);
                if(!Spatial.Query.SelfIntersect(polygon3D))
                {
                    polygon3Ds.Add(polygon3D);
                    continue;
                }

                List<Polygon3D> polygon3Ds_Intersection = Spatial.Query.SelfIntersectionPolygon2Ds(polygon3D);
                if (polygon3Ds_Intersection != null)
                    polygon3Ds.AddRange(polygon3Ds_Intersection);
            }
                

            return polygon3Ds;
        }
    }
}
