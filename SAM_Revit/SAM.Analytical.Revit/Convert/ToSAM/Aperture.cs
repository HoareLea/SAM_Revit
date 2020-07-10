using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Aperture ToSAM(this EnergyAnalysisOpening energyAnalysisOpening)
        {
            if (energyAnalysisOpening == null)
                return null;

            Polygon3D polygon3D = energyAnalysisOpening.GetPolyloop().ToSAM();
            if (polygon3D == null)
                return null;

            FamilyInstance familyInstance = Core.Revit.Query.Element(energyAnalysisOpening) as FamilyInstance;
            if(familyInstance == null)
                return new Aperture(null, polygon3D);

            Aperture result;

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                result = Core.Revit.Query.IJSAMObject<Aperture>(familyInstance);
                if (result != null)
                    return result;
            }

            ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction(familyInstance);

            Point3D point3D_Location = Geometry.Revit.Query.Location(familyInstance);
            if (point3D_Location == null)
                return null;

            result = new Aperture(apertureConstruction, polygon3D, point3D_Location);
            result.Add(Core.Revit.Query.ParameterSet(familyInstance));
            return result;
        }

        public static Aperture ToSAM_Aperture(this FamilyInstance familyInstance)
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

            Point3D point3D_Location = Geometry.Revit.Query.Location(familyInstance);
            if (point3D_Location == null)
                return null;

            PanelType panelType_Host = PanelType.Undefined;
            BuiltInCategory builtInCategory_Host = BuiltInCategory.INVALID;
            if (familyInstance.Host != null)
            {
                HostObject hostObject = familyInstance.Host as HostObject;
                if (hostObject != null)
                {
                    builtInCategory_Host = (BuiltInCategory)hostObject.Category.Id.IntegerValue;

                    List<Face3D> face3Ds_Temp = hostObject.Profiles();
                    if(face3Ds_Temp != null && face3Ds_Temp.Count != 0)
                    {
                        Geometry.Spatial.Plane plane_Host = face3Ds_Temp.Closest(point3D_Location)?.GetPlane();
                        if (plane_Host != null)
                            point3D_Location = plane_Host.Project(point3D_Location);
                    }

                    HostObjAttributes hostObjAttributes = familyInstance.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
                    if(hostObjAttributes != null)
                        panelType_Host = hostObjAttributes.PanelType();

                    if (panelType_Host == PanelType.Undefined)
                        panelType_Host = hostObject.PanelType();
                }
            }

            ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction(familyInstance);
            if (apertureConstruction == null && panelType_Host != PanelType.Undefined)
                apertureConstruction = Analytical.Query.ApertureConstruction(panelType_Host, familyInstance.ApertureType()); //Default Aperture Construction

            Vector3D axisX = null;
            Vector3D normal = null;
            Vector3D axisY = null;
            if (builtInCategory_Host == BuiltInCategory.OST_Roofs)
            {
                axisX = familyInstance.HandOrientation.ToSAM_Vector3D(false);
                axisY = familyInstance.FacingOrientation.ToSAM_Vector3D(false);
                normal = Geometry.Spatial.Query.AxisY(axisY, axisX);
            }
            else
            {
                axisX = familyInstance.HandOrientation.ToSAM_Vector3D(false);
                normal = familyInstance.FacingOrientation.ToSAM_Vector3D(false);
                axisY = Geometry.Spatial.Query.AxisY(normal, axisX);
            }


            Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(point3D_Location, axisX, axisY);
            if (!plane.Normal.SameHalf(normal))
                plane.FlipZ(false);

            List<Face3D> face3Ds = Geometry.Revit.Convert.ToSAM_Face3Ds(familyInstance);
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Face3D face3D_Temp in face3Ds)
            {
                IClosedPlanar3D closedPlanar3D = face3D_Temp.GetExternalEdge();
                if (closedPlanar3D is ICurvable3D)
                {
                    List<ICurve3D> curve3Ds = ((ICurvable3D)closedPlanar3D).GetCurves();
                    foreach (ICurve3D curve3D in curve3Ds)
                    {
                        ICurve3D curve3D_Temp = plane.Project(curve3D);
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

            aperture = new Aperture(apertureConstruction, new Face3D(plane, rectangle2D));
            aperture.Add(Core.Revit.Query.ParameterSet(familyInstance));

            return aperture;
        }
    }
}