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
            return new Segment3D(line.GetEndPoint(0).ToSAM(), line.GetEndPoint(2).ToSAM());
        }
    }
}
