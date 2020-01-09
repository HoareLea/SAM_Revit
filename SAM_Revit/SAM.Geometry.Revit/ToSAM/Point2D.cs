using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Planar;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Point2D ToSAM(this UV uV)
        {
            double scale = Units.Convert.ToSI(1, Units.UnitType.Feet);

            return new Point2D(uV.U * scale, uV.V * scale);
        }
    }
}
