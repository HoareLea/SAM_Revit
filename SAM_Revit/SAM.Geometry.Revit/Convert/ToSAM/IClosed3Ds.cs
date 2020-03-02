using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<IClosed3D> ToSAM(this Sketch sketch)
        {
            if (sketch == null || sketch.Profile == null)
                return null;

            List<IClosed3D> result = new List<IClosed3D>();
            foreach (CurveArray curveArray in sketch.Profile)
                result.Add(curveArray.ToSAM());

            return result;
        }
    }
}
