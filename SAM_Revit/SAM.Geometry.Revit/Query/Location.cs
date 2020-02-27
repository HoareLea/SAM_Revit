using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Point3D Location(this Element element)
        {
            if (element == null)
                return null;
            
            if (element.Location is LocationPoint)
                return ((LocationPoint)element.Location).Point.ToSAM();

            if (element.Location is LocationCurve)
                return ((LocationCurve)element.Location).Curve.GetEndPoint(0).ToSAM();

            return null;
        }
    }
}
