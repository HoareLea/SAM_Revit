using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Space ToSAM(this SpatialElement spatialElement)
        {
            Point3D point3D = null;

            if (spatialElement.Location != null)
                point3D = ((LocationPoint)spatialElement.Location).Point.ToSAM();

            return new Space(spatialElement.Name, point3D);
        }
    }
}
