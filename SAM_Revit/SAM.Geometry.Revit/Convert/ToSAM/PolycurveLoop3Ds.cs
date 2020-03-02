using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<PolycurveLoop3D> ToSAM(this IEnumerable<CurveLoop> curveLoops)
        {
            List<PolycurveLoop3D> result = new List<PolycurveLoop3D>();
            foreach (CurveLoop curveLoop in curveLoops)
                result.Add(curveLoop.ToSAM());

            return result;
        }

        public static List<PolycurveLoop3D> ToSAM_PolycurveLoop3Ds(this Autodesk.Revit.DB.Face face)
        {
            return ToSAM(face.GetEdgesAsCurveLoops());
        }
    
        public static List<PolycurveLoop3D> ToSAM_PolycurveLoop3D(this Mesh mesh)
        {
            List<PolycurveLoop3D> result = new List<PolycurveLoop3D>();
            for (int i = 0; i < mesh.NumTriangles; i++)
                result.Add(new PolycurveLoop3D(mesh.get_Triangle(i).ToSAM()));

            return result;
        }
    }
}
