using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Segment3D> ToSAM_Segment3Ds(this Curve curve)
        {
            if (curve == null)
                return null;
            
            if (curve is Line)
                return new List<Segment3D>() { ToSAM((Line)curve) };

            ISegmentable3D segmentable3D = ToSAM(curve) as ISegmentable3D;
            if (segmentable3D == null)
                return null;

            List<Segment3D> result = segmentable3D.GetSegments();
            if(!curve.IsBound && result.Count > 1 && result[0][0] != result[result.Count - 1][1])
                result.Add(new Segment3D(result[result.Count - 1][1], result[0][0]));

            return result;

            //List<Point3D> point3Ds = curve.Tessellate()?.ToList().ConvertAll(x => x.ToSAM());
            //if (point3Ds == null || point3Ds.Count() == 0)
            //    return new List<Segment3D>() { ToSAM_Segment3D(curve) };

            //for (int i = 0; i < point3Ds.Count - 1; i++)
            //    result.Add(new Segment3D(point3Ds[i], point3Ds[i + 1]));

            //if (!curve.IsBound && point3Ds.Count > 2)
            //    result.Add(new Segment3D(point3Ds.Last(), point3Ds.First()));
        }
    }
}