using Autodesk.Revit.DB;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Point2D ToSAM(this UV uV)
        {
            return new Point2D(uV.U * Units.Revit.ConversionFactor.FromFeetToMeter, uV.V * Units.Revit.ConversionFactor.FromFeetToMeter);
        }
    }
}