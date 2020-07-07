using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static FamilyInstance ToRevit(this Aperture aperture, Document document, HostObject hostObject, Core.Revit.ConvertSettings convertSettings)
        {
            if (aperture == null || document == null)
                return null;

            ApertureConstruction apertureConstruction = aperture.ApertureConstruction;
            if (apertureConstruction == null)
                return null;

            FamilySymbol familySymbol = apertureConstruction.ToRevit(document, convertSettings);
            if (familySymbol == null)
                familySymbol = Analytical.Query.ApertureConstruction(apertureConstruction.ApertureType, true).ToRevit(document, convertSettings); //Default Aperture Construction

            if (familySymbol == null)
                return null;

            Geometry.Spatial.Point3D point3D_Location = aperture.PlanarBoundary3D?.Plane?.Origin;
            if (point3D_Location == null)
                return null;

            Level level = Geometry.Revit.Query.LowLevel(document, point3D_Location.Z);
            if (level == null)
                return null;

            FamilyInstance familyInstance;
            if (hostObject is RoofBase)
                familyInstance = document.Create.NewFamilyInstance(point3D_Location.ToRevit(), familySymbol, aperture.Plane.AxisX.ToRevit(false), hostObject, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            else
                familyInstance = document.Create.NewFamilyInstance(point3D_Location.ToRevit(), familySymbol, hostObject, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            if (familyInstance == null)
                return null;

            if (familyInstance.CanFlipHand)
            {
                document.Regenerate();
                Geometry.Spatial.Vector3D axisX = familyInstance.HandOrientation.ToSAM_Vector3D(false);
                if (!axisX.SameHalf(aperture.Plane.AxisX))
                    familyInstance.flipHand();
            }

            if (familyInstance.CanFlipFacing)
            {
                document.Regenerate(); // this is needed to get flip correctly pushed to revit
                Geometry.Spatial.Vector3D normal = familyInstance.FacingOrientation.ToSAM_Vector3D(false);
                if (!normal.SameHalf(aperture.Plane.Normal))
                    familyInstance.flipFacing();
            }

            if (convertSettings.ConvertParameters)
            {
                Core.Revit.Modify.Values(aperture, familyInstance, new BuiltInParameter[] { BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM, BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM, BuiltInParameter.FAMILY_LEVEL_PARAM, BuiltInParameter.SCHEDULE_LEVEL_PARAM });
                Core.Revit.Modify.Values(ActiveSetting.Setting, aperture, familyInstance);

                bool simplified = false;
                if (!Geometry.Planar.Query.Rectangular(aperture.PlanarBoundary3D?.Edge2DLoop?.GetClosed2D()))
                    simplified = true;

                Core.Revit.Modify.Simplified(familyInstance, simplified);
                Core.Revit.Modify.Json(familyInstance, aperture.ToJObject()?.ToString());
            }

            return familyInstance;
        }
    }
}