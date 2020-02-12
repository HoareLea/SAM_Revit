using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Face3D> ToSAM_Faces(this Autodesk.Revit.DB.Face face)
        {
            return face.ToSAM_Polygon3Ds().ConvertAll(x => new Face3D(x));
        }

        public static List<Face3D> ToSAM_Faces(this Sketch sketch)
        {
            if (sketch == null || sketch.Profile == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            foreach (CurveArray curveArray in sketch.Profile)
                result.Add(curveArray.ToSAM_Face());

            return result;
        }
    }
}
