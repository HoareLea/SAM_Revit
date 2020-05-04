using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Aperture ToSAM_Aperture(this FamilyInstance familyInstance, Geometry.Spatial.Plane plane, PanelType panelType)
        {
            if (familyInstance == null || plane == null)
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

            point3D_Location = plane.Project(point3D_Location);

            Geometry.Spatial.Plane plane_Location = new Geometry.Spatial.Plane(point3D_Location, plane.Normal);

            List<Geometry.Spatial.Face3D> face3Ds = Geometry.Revit.Convert.ToSAM_Face3Ds(familyInstance);
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
            foreach (Geometry.Spatial.Face3D face3D_Temp in face3Ds)
            {
                Geometry.Spatial.IClosedPlanar3D closedPlanar3D = face3D_Temp.GetExternalEdge();
                if (closedPlanar3D is Geometry.Spatial.ICurvable3D)
                {
                    List<Geometry.Spatial.ICurve3D> curve3Ds = ((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves();
                    foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                    {
                        Geometry.Spatial.ICurve3D curve3D_Temp = plane_Location.Project(curve3D);
                        point2Ds.Add(plane_Location.Convert(curve3D_Temp.GetStart()));
                    }
                }
            }

            Geometry.Planar.Rectangle2D rectangle2D = Geometry.Planar.Point2D.GetRectangle2D(point2Ds);

            aperture = new Aperture(apertureConstruction, new Geometry.Spatial.Face3D(plane_Location, rectangle2D));
            aperture.Add(Core.Revit.Query.ParameterSet(familyInstance));

            return aperture;
        }
    }
}