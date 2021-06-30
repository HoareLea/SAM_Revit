using Autodesk.Revit.DB;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static List<Polygon3D> Polygon3Ds(this CurtainCell curtainCell)
        {
            CurveArrArray curveArrArray = curtainCell?.PlanarizedCurveLoops;
            if(curveArrArray == null)
            {
                return null;
            }

            List<Polygon3D> result = new List<Polygon3D>();
            foreach (CurveArray curveArray in curtainCell.PlanarizedCurveLoops)
            {
                Polygon3D polygon3D = curveArray?.ToSAM_Polygon3D();
                if (polygon3D == null && !polygon3D.IsValid())
                {
                    continue;
                }


                Geometry.Spatial.Plane plane = polygon3D.GetPlane();
                if (plane == null)
                {
                    continue;
                }

                result.Add(polygon3D);
            }

            return result;
        }
    }
}