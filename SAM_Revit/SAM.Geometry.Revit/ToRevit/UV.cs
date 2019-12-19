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
            return new UV(point2D.X, point2D.Y);
        }
    }
}
