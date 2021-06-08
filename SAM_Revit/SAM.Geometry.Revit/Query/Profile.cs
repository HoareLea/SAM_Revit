using SAM.Geometry.Spatial;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static IClosedPlanar3D Profile(this MEPCurve mEPCurve)
        {
            Spatial.Plane plane = mEPCurve?.Plane();
            if (plane == null)
            {
                return null;
            }

            switch (mEPCurve.ConnectorProfileType())
            {
                case Autodesk.Revit.DB.ConnectorProfileType.Rectangular:

                    double width = Units.Convert.ToSI(mEPCurve.Width, Units.UnitType.Feet);
                    double height = Units.Convert.ToSI(mEPCurve.Height, Units.UnitType.Feet);

                    Planar.Point2D origin = new Planar.Point2D(-width / 2, -height / 2);
                    Planar.Vector2D heightDirection = plane.Convert(plane.AxisY);

                    return new Rectangle3D(plane, new Planar.Rectangle2D(origin, width, height, heightDirection));


                case Autodesk.Revit.DB.ConnectorProfileType.Round:

                    double diameter = Units.Convert.ToSI(mEPCurve.Diameter, Units.UnitType.Feet);
                    double radius = diameter / 2;

                    return new Circle3D(plane, radius);

                case Autodesk.Revit.DB.ConnectorProfileType.Oval:

                    throw new System.NotImplementedException();
            }

            return null;
        }
    }
}