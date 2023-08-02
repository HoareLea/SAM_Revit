using Autodesk.Revit.DB;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static UV ToRevit(this Point2D point2D)
        {
            return new UV(point2D.X * Units.Revit.ConversionFactor.FromMeterToFeet, point2D.Y * Units.Revit.ConversionFactor.FromMeterToFeet);
        }
    }
}