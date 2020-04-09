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
        public static Segment3D ToSAM(this Line line)
        {
            return new Segment3D(line.GetEndPoint(0).ToSAM(), line.GetEndPoint(1).ToSAM());
        }

        public static Segment3D ToSAM_Segment3D(this Curve curve)
        {
            return new Segment3D(curve.GetEndPoint(0).ToSAM(), curve.GetEndPoint(1).ToSAM());
        }
    }
}
