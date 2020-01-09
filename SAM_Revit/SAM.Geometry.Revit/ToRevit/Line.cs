using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;


namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Line ToRevit(this Segment3D segment3D)
        {
            return Line.CreateBound(segment3D.GetStart().ToRevit(), segment3D.GetEnd().ToRevit());
        }
    }
}
