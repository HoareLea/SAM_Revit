using Autodesk.Revit.DB;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Aperture ToSAM_Aperture(this FamilyInstance familyInstance, PanelType panelType)
        {
            if (familyInstance == null)
                return null;

            Aperture aperture = null;

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                aperture = Core.Revit.Query.IJSAMObject<Aperture>(familyInstance);
                if (aperture != null)
                    return aperture;
            }

            ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction(familyInstance);
            if (apertureConstruction == null)
                apertureConstruction = Analytical.Query.ApertureConstruction(panelType, familyInstance.ApertureType()); //Default Aperture Construction

            Geometry.Spatial.Point3D point3D_Location = Geometry.Revit.Query.Location(familyInstance);
            if (point3D_Location == null)
                return null;

            Geometry.Spatial.Vector3D axisX = familyInstance.HandOrientation.ToSAM_Vector3D(true);
            Geometry.Spatial.Vector3D normal = familyInstance.FacingOrientation.ToSAM_Vector3D(true);

            Geometry.Spatial.Vector3D axisY = Geometry.Spatial.Query.AxisY(normal, axisX);

            Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(point3D_Location, axisX, axisY);

            List<Geometry.Spatial.Face3D> face3Ds = Geometry.Revit.Convert.ToSAM_Face3Ds(familyInstance);
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Geometry.Spatial.Face3D face3D_Temp in face3Ds)
            {
                Geometry.Spatial.IClosedPlanar3D closedPlanar3D = face3D_Temp.GetExternalEdge();
                if (closedPlanar3D is Geometry.Spatial.ICurvable3D)
                {
                    List<Geometry.Spatial.ICurve3D> curve3Ds = ((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves();
                    foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                    {
                        Geometry.Spatial.ICurve3D curve3D_Temp = plane.Project(curve3D);
                        point2Ds.Add(plane.Convert(curve3D_Temp.GetStart()));
                    }
                }
            }

            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            //TODO: Working on SAM Families (requested by Michal)

            string parameterName_Height = Query.ParameterName_ApertureHeight();
            string parameterName_Width = Query.ParameterName_BuildingElementWidth();
            if (!string.IsNullOrWhiteSpace(parameterName_Height) && !string.IsNullOrWhiteSpace(parameterName_Width))
            {
                Parameter parameter_Height = familyInstance.LookupParameter(parameterName_Height);
                Parameter parameter_Width = familyInstance.LookupParameter(parameterName_Width);
                if (parameter_Height != null && parameter_Width != null && parameter_Height.HasValue && parameter_Width.HasValue && parameter_Height.StorageType == StorageType.Double && parameter_Width.StorageType == StorageType.Double)
                {
                    double height = UnitUtils.ConvertFromInternalUnits(parameter_Height.AsDouble(), DisplayUnitType.DUT_METERS);
                    double width = UnitUtils.ConvertFromInternalUnits(parameter_Width.AsDouble(), DisplayUnitType.DUT_METERS);

                    BoundingBox2D boundingBox2D = new BoundingBox2D(point2Ds);
                    double factor_Height = height / boundingBox2D.Height;
                    double factor_Width = width / boundingBox2D.Width;

                    point2Ds = point2Ds.ConvertAll(x => new Point2D(x.X * factor_Width, x.Y * factor_Height));
                }
            }

            Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(point2Ds);

            aperture = new Aperture(apertureConstruction, new Geometry.Spatial.Face3D(plane, rectangle2D));
            aperture.Add(Core.Revit.Query.ParameterSet(familyInstance));

            return aperture;
        }
    }
}