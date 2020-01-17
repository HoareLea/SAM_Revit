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
        public static UV ToRevit(this Point2D point2D)
        {
            double scale = Units.Query.ToImperial(1, Units.UnitType.Meter);

            return new UV(point2D.X * scale, point2D.Y * scale);
        }
    }
}
