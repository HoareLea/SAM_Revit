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
        public static Point3D ToSAM(this XYZ XYZ)
        {
            return new Point3D(XYZ.X, XYZ.Y, XYZ.Z);
        }
    }
}
