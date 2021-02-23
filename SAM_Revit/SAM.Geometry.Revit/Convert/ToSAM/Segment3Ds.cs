using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

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

            List<Point3D> point3Ds = curve.Tessellate()?.ToList().ConvertAll(x => x.ToSAM());
            if (point3Ds == null || point3Ds.Count() == 0)
                return new List<Segment3D>() { ToSAM_Segment3D(curve) };

            List<Segment3D> result = new List<Segment3D>();

            for (int i = 0; i < point3Ds.Count - 1; i++)
                result.Add(new Segment3D(point3Ds[i], point3Ds[i + 1]));

            if (!curve.IsBound && point3Ds.Count > 2)
                result.Add(new Segment3D(point3Ds.Last(), point3Ds.First()));

            return result;
        }
    }
}