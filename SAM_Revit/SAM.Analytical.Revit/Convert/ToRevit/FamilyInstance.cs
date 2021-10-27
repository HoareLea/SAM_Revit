using Autodesk.Revit.DB;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

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
                familySymbol = Analytical.Query.DefaultApertureConstruction(hostObject.PanelType(), apertureConstruction.ApertureType).ToRevit(document, convertSettings); //Default Aperture Construction

            if (familySymbol == null)
                return null;

            Point3D point3D_Location = aperture.PlanarBoundary3D?.Plane?.Origin;
            if (point3D_Location == null)
                return null;

            Level level = Geometry.Revit.Query.LowLevel(document, point3D_Location.Z);
            if (level == null)
                return null;

            XYZ referenceDirection = new XYZ(0, 0, 0);

            if (hostObject is RoofBase)
            {

                Face3D face3D = aperture.GetFace3D();
                Geometry.Spatial.Plane plane = face3D.GetPlane();

                bool coplanar = plane.Coplanar(Geometry.Spatial.Plane.WorldXY);
                //if(coplanar)
                //{
                //    referenceDirection = new XYZ(0, 0, 1);
                //}

                result = document.Create.NewFamilyInstance(point3D_Location.ToRevit(), familySymbol, referenceDirection, hostObject, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                if (result == null)
                    return null;

                //List<Geometry.Planar.Point2D> point2Ds = new List<Geometry.Planar.Point2D>();
                //IClosedPlanar3D closedPlanar3D = face3D.GetExternalEdge3D();
                //if (closedPlanar3D is ICurvable3D)
                //{
                //    List<ICurve3D> curve3Ds = ((ICurvable3D)closedPlanar3D).GetCurves();
                //    foreach (ICurve3D curve3D in curve3Ds)
                //    {
                //        ICurve3D curve3D_Temp = plane.Project(curve3D);
                //        point2Ds.Add(plane.Convert(curve3D_Temp.GetStart()));
                //    }
                //}

                //Geometry.Planar.Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(point2Ds);
                Geometry.Planar.Rectangle2D rectangle2D = Analytical.Create.Rectangle2D(aperture.PlanarBoundary3D);
                if (rectangle2D == null)
                    return null;

                document.Regenerate();
                result = document.GetElement(result.Id) as FamilyInstance;

                Vector3D handOrientation_FamilyInstance = result.HandOrientation.ToSAM_Vector3D(false);
                Vector3D facingOrientation_FamilyInstance = result.FacingOrientation.ToSAM_Vector3D(false);


                double factor = 0;
                Geometry.Planar.Vector2D direction = rectangle2D.WidthDirection;
                if (!coplanar && Core.Query.Round(direction.Y) < 0)
                    factor = System.Math.PI / 2;

                Vector3D handOrienation_Aperture = plane.Convert(direction);

                Geometry.Spatial.Plane plane_FamilyInstance = new Geometry.Spatial.Plane(point3D_Location, handOrientation_FamilyInstance, facingOrientation_FamilyInstance);
                handOrienation_Aperture = plane_FamilyInstance.Project(handOrienation_Aperture);

                double angle = Geometry.Spatial.Query.SignedAngle(handOrientation_FamilyInstance, handOrienation_Aperture, plane.Normal);

                result.Location.Rotate(Line.CreateUnbound(point3D_Location.ToRevit(), plane.Normal.ToRevit(false)), angle + factor);
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
                document.Regenerate(); //This is needed to get flip correctly pushed to revit
                Vector3D axisX = result.HandOrientation.ToSAM_Vector3D(false);
                if (!axisX.SameHalf(aperture.Plane.AxisX))
                    result.flipHand();
            }

            if (result.CanFlipFacing)
            {
                document.Regenerate(); //This is needed to get flip correctly pushed to revit
                Vector3D normal = result.FacingOrientation.ToSAM_Vector3D(false);
                if (!normal.SameHalf(aperture.Plane.Normal))
                    result.flipFacing();
            }

            if (convertSettings.ConvertParameters)
            {
                Core.Revit.Modify.SetValues(result, aperture, new BuiltInParameter[] { BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM, BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM, BuiltInParameter.FAMILY_LEVEL_PARAM, BuiltInParameter.SCHEDULE_LEVEL_PARAM });
                Core.Revit.Modify.SetValues(result, aperture, ActiveSetting.Setting);

                bool simplified = false;

                //Check if geometry is simplified
                if (!Geometry.Planar.Query.Rectangular(aperture.PlanarBoundary3D?.ExternalEdge2DLoop?.GetClosed2D(), Core.Tolerance.MacroDistance))
                    simplified = true;

                if (!simplified && result.Host is Autodesk.Revit.DB.Wall)
                {
                    Face3D face3D = aperture.GetFace3D();
                    Geometry.Spatial.Plane plane = face3D.GetPlane();

                    Geometry.Planar.Rectangle2D rectangle2D = Analytical.Create.Rectangle2D(aperture.PlanarBoundary3D);
                    if (rectangle2D != null)
                    {
                        Vector3D widthDirection = plane.Convert(rectangle2D.WidthDirection);
                        Vector3D heightDirection = plane.Convert(rectangle2D.HeightDirection);

                        //TODO: Implement code for Tilted Walls
                        Vector3D vector3D_Z = Vector3D.WorldZ;
                        
                        if (!widthDirection.AlmostSimilar(vector3D_Z) && !heightDirection.AlmostSimilar(vector3D_Z))
                            simplified = true;
                    }
                }

                Core.Revit.Modify.SetSimplified(result, simplified);
                Core.Revit.Modify.SetJson(result, aperture.ToJObject()?.ToString());
            }

            convertSettings?.Add(aperture.Guid, result);

            return result;
        }

        public static FamilyInstance ToRevit(this Aperture aperture, Document document, Core.Revit.ConvertSettings convertSettings)
        {
            if (aperture == null || document == null)
                return null;
            
            HostObject hostObject = Query.HostObject(aperture, document);
            if (hostObject == null)
                return null;

            return ToRevit(aperture, document, hostObject, convertSettings);
        }
    }
}