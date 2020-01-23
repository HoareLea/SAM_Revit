using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Spatial.Face ToSAM_Face(this CurveArray curveArray)
        {
            return new Spatial.Face(curveArray.ToSAM_Polygon3D());
        }
    }
}
