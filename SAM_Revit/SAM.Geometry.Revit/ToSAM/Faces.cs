using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Spatial.Face> ToSAM_Faces(this Autodesk.Revit.DB.Face face)
        {
            return face.ToSAM_Polygon3Ds().ConvertAll(x => new Spatial.Face(x));
        }

        public static List<Spatial.Face> ToSAM_Faces(this Sketch sketch)
        {
            if (sketch == null || sketch.Profile == null)
                return null;

            List<Spatial.Face> result = new List<Spatial.Face>();
            foreach (CurveArray curveArray in sketch.Profile)
                result.Add(curveArray.ToSAM_Face());

            return result;
        }
    }
}
