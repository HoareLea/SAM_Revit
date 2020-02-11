using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Spatial.Face3D ToSAM_Face(this CurveArray curveArray)
        {
            return new Spatial.Face3D(curveArray.ToSAM_Polygon3D());
        }
    }
}
