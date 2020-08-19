using Autodesk.Revit.DB;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static FamilyInstance ToRevit(this Aperture aperture, Document document, HostObject hostObject, Core.Revit.ConvertSettings convertSettings)
        {
            if (aperture == null || document == null)
                return null;

            FamilyInstance result = convertSettings?.GetObject<FamilyInstance>(aperture.Guid);
            if (result != null)
                return result;

            ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
            if (apertureConstruction == null)
                return null;

            FamilySymbol familySymbol = apertureConstruction.ToRevit(document, convertSettings);
            if (familySymbol == null)
                familySymbol = Analytical.Query.ApertureConstruction(apertureConstruction.ApertureType, true).ToRevit(document, convertSettings); //Default Aperture Construction

            if (familySymbol == null)
                return null;

            Point3D point3D_Location = aperture.PlanarBoundary3D?.Plane?.Origin;
            if (point3D_Location == null)
                return null;

            Level level = Geometry.Revit.Query.LowLevel(document, point3D_Location.Z);
            if (level == null)
                return null;

            if (hostObject is RoofBase)
            {
                result = document.Create.NewFamilyInstance(point3D_Location.ToRevit(), familySymbol, new XYZ(0, 0, 0), hostObject, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                if (result == null)
                    return null;              
                
                Face3D face3D = aperture.GetFace3D();
                Geometry.Spatial.Plane plane = face3D.GetPlane();
                
                List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
                IClosedPlanar3D closedPlanar3D = face3D.GetExternalEdge3D();
                if (closedPlanar3D is ICurvable3D)
                {
                    List<ICurve3D> curve3Ds = ((ICurvable3D)closedPlanar3D).GetCurves();
                    foreach (ICurve3D curve3D in curve3Ds)
                    {
                        ICurve3D curve3D_Temp = plane.Project(curve3D);
                        point2Ds.Add(plane.Convert(curve3D_Temp.GetStart()));
                    }
                }

                Geometry.Planar.Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(point2Ds);
                if (rectangle2D == null)
                    return null;

                document.Regenerate();
                result = document.GetElement(result.Id) as FamilyInstance;

                Vector3D handOrientation_FamilyInstance = result.HandOrientation.ToSAM_Vector3D(false);
                Vector3D facingOrientation_FamilyInstance = result.FacingOrientation.ToSAM_Vector3D(false);

                Vector3D handOrienation_Aperture = plane.Convert(rectangle2D.WidthDirection);

                Geometry.Spatial.Plane plane_FamilyInstance = new Geometry.Spatial.Plane(point3D_Location, handOrientation_FamilyInstance, facingOrientation_FamilyInstance);
                handOrienation_Aperture = plane_FamilyInstance.Project(handOrienation_Aperture);

                double angle = Geometry.Spatial.Query.SignedAngle(handOrientation_FamilyInstance, handOrienation_Aperture, plane.Normal);

                result.Location.Rotate(Line.CreateUnbound(point3D_Location.ToRevit(), plane.Normal.ToRevit(false)), angle);
                //document.Regenerate();

                //BoundingBox3D boundingBox3D_familyInstance = familyInstance.BoundingBox3D();
                //BoundingBox3D boundingBox3D_Aperture = aperture.GetBoundingBox();
                //if(boundingBox3D_familyInstance.Min.Distance(boundingBox3D_Aperture.Min) > SAM.Core.Tolerance.MacroDistance)
                //    familyInstance.Location.Rotate(Line.CreateUnbound(point3D_Location.ToRevit(), plane.Normal.ToRevit(false)), System.Math.PI / 2);

                //Geometry.Planar.Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(point2Ds);
                //Geometry.Planar.Vector2D direction = null;
                //if (rectangle2D.Height > rectangle2D.Width)
                //    direction = rectangle2D.HeightDirection;
                //else
                //    direction = rectangle2D.WidthDirection;

                //double angle = plane.Convert(direction).ToRevit(false).AngleTo(new XYZ(0, 1, 0));
                //angle = System.Math.PI  - angle;
                ////if (angle > System.Math.PI)
                ////    angle = -(angle - System.Math.PI);
                //if (direction.X < 0)
                //    angle = -angle;

                //familyInstance.Location.Rotate(Line.CreateUnbound(point3D_Location.ToRevit(), plane.Normal.ToRevit(false)), angle);
            }
            else
            {
                result = document.Create.NewFamilyInstance(point3D_Location.ToRevit(), familySymbol, hostObject, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            }
                

            if (result == null)
                return null;

            if (result.CanFlipHand)
            {
                document.Regenerate();
                Geometry.Spatial.Vector3D axisX = result.HandOrientation.ToSAM_Vector3D(false);
                if (!axisX.SameHalf(aperture.Plane.AxisX))
                    result.flipHand();
            }

            if (result.CanFlipFacing)
            {
                document.Regenerate(); // this is needed to get flip correctly pushed to revit
                Geometry.Spatial.Vector3D normal = result.FacingOrientation.ToSAM_Vector3D(false);
                if (!normal.SameHalf(aperture.Plane.Normal))
                    result.flipFacing();
            }

            if (convertSettings.ConvertParameters)
            {
                Core.Revit.Modify.Values(aperture, result, new BuiltInParameter[] { BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM, BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM, BuiltInParameter.FAMILY_LEVEL_PARAM, BuiltInParameter.SCHEDULE_LEVEL_PARAM });
                Core.Revit.Modify.Values(ActiveSetting.Setting, aperture, result);

                bool simplified = false;
                //check if geometry is rectagular
                if (!Geometry.Planar.Query.Rectangular(aperture.PlanarBoundary3D?.Edge2DLoop?.GetClosed2D(), Core.Tolerance.MacroDistance))
                    simplified = true;

                Core.Revit.Modify.Simplified(result, simplified);
                Core.Revit.Modify.Json(result, aperture.ToJObject()?.ToString());
            }

            convertSettings?.Add(aperture.Guid, result);

            return result;
        }
    }
}