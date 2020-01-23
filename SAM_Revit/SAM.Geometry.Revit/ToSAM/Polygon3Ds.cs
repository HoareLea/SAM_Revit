using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Polygon3D> ToSAM_Polygon3Ds(this Autodesk.Revit.DB.Face face)
        {
            return ToSAM_Polygon3Ds(face.GetEdgesAsCurveLoops());
        }

        public static List<Polygon3D> ToSAM_Polygon3Ds(this IEnumerable<CurveLoop> curveLoops)
        {
            List<Polygon3D> result = new List<Polygon3D>();
            foreach (CurveLoop curveLoop in curveLoops)
                result.Add(ToSAM_Polygon3D(curveLoop));

            return result;
        }
    }
}
