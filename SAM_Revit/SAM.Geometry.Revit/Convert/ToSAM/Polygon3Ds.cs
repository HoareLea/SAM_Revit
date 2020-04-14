using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

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
                polygon3Ds.Add(ToSAM_Polygon3D(curveLoop));

            if (polygon3Ds == null)
                return null;

            if (polygon3Ds.Count == 0)
                return polygon3Ds;

            Spatial.Plane plane = polygon3Ds.First().GetPlane();

            List<Planar.Polygon2D> polygon2Ds = polygon3Ds.ConvertAll(x => plane.Convert(x));

            List<Planar.Segment2D> segment2Ds = new List<Planar.Segment2D>();
            polygon2Ds.ForEach(x => segment2Ds.AddRange(x.GetSegments()));

            Planar.PointGraph2D pointGraph2D = new Planar.PointGraph2D(segment2Ds, true);

            polygon2Ds = pointGraph2D.GetPolygon2Ds();
            if (polygon3Ds != null && polygon3Ds.Count <= 1)
                return polygon2Ds.ConvertAll(x => plane.Convert(x));

            polygon2Ds.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
            polygon2Ds.RemoveAt(polygon3Ds.Count - 1);

            List<Planar.Polygon2D> polygon2Ds_External = pointGraph2D.GetPolygon2Ds_External();
            if (polygon2Ds_External == null || polygon2Ds_External.Count == 0)
                return null;

            polygon2Ds_External.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));

            polygon3Ds.Add(plane.Convert(polygon2Ds_External.Last()));
            polygon2Ds.ForEach(x => polygon3Ds.Add(plane.Convert(x)));

            return polygon3Ds;
        }
    }
}
