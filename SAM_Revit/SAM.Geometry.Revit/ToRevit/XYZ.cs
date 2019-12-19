using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static XYZ ToRevit(this Point3D point3D)
        {
            return new XYZ(point3D.X, point3D.Y, point3D.Z);
        }
    }
}
