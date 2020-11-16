using Autodesk.Revit.DB;
using SAM.Core;
using System.Reflection;

namespace SAM.Analytical.Revit
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            public const string FileName_DefaultAirConstructionLibrary = "FileName_DefaultAirConstructionLibrary";
            public const string Library_DefaultAirConstructionLibrary = "Library_DefaultAirConstructionLibrary";
        }

        private static Setting setting = Load();

        private static Setting Load()
        {
            Setting setting = ActiveManager.GetSetting(Assembly.GetExecutingAssembly());
            if (setting == null)
                setting = GetDefault();

            return setting;
        }

        public static Setting Setting
        {
            get
            {
                return setting;
            }
        }

        public static Setting GetDefault()
        {
            Setting result = new Setting(Assembly.GetExecutingAssembly());

            MapCluster mapCluster = new MapCluster();

            //AnalyticalModel
            mapCluster.Add(AnalyticalModelParameter.NorthAngle, typeof(ProjectInfo), "SAM_NorthAngle");

            //Aperture
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), "GetWidth", "SAM_BuildingElementWidth");
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), "GetHeight", "SAM_BuildingElementHeight");
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), "Guid", "SAM_GUID");

            //ApertureConstruction
            mapCluster.Add(ApertureConstructionParameter.Color, typeof(FamilySymbol), "SAM_BuildingElementColor");
            mapCluster.Add(ApertureConstructionParameter.DefaultPanelType, typeof(FamilySymbol), "SAM_BuildingElementType");
            mapCluster.Add(ApertureConstructionParameter.Transparent, typeof(FamilySymbol), "SAM_BuildingElementTransparent");
            mapCluster.Add(ApertureConstructionParameter.Description, typeof(FamilySymbol), "SAM_BuildingElementDescription");
            mapCluster.Add(ApertureConstructionParameter.DefaultFrameWidth, typeof(FamilySymbol), "SAM_BuildingElementFrameWidth");
            mapCluster.Add(ApertureConstructionParameter.IsInternalShadow, typeof(FamilySymbol), "SAM_BuildingElementInternalShadows");

            //Construction
            mapCluster.Add(ConstructionParameter.Color, typeof(HostObjAttributes), "SAM_BuildingElementColor");
            mapCluster.Add(ConstructionParameter.DefaultPanelType, typeof(HostObjAttributes), "SAM_BuildingElementType");
            mapCluster.Add(ConstructionParameter.Description, typeof(HostObjAttributes), "SAM_BuildingElementDescription");
            mapCluster.Add(ConstructionParameter.IsAir, typeof(HostObjAttributes), "SAM_BuildingElementAir");
            mapCluster.Add(ConstructionParameter.IsInternalShadow, typeof(HostObjAttributes), "SAM_BuildingElementInternalShadows");
            mapCluster.Add(ConstructionParameter.IsGround, typeof(HostObjAttributes), "SAM_BuildingElementGround");
            mapCluster.Add(ConstructionParameter.Transparent, typeof(HostObjAttributes), "SAM_BuildingElementTransparent");
            mapCluster.Add(ConstructionParameter.DefaultThickness, typeof(HostObjAttributes), "SAM_BuildingElementThickness");

            //InternalCondition
            mapCluster.Add(InternalConditionParameter.NumberOfPeople, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_NoPeople");
            mapCluster.Add(InternalConditionParameter.OccupancyProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupacyProfile");
            mapCluster.Add(InternalConditionParameter.OccupantSensibleGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantSensGain");
            mapCluster.Add(InternalConditionParameter.OccupantLatentGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantLatGain");
            mapCluster.Add(InternalConditionParameter.EquipmentSensibleProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SmallPowerSensProfile");
            mapCluster.Add(InternalConditionParameter.EquipmentLatentProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SmallPowerLatProfile");
            mapCluster.Add(InternalConditionParameter.LightingGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_GenLightingGain");
            mapCluster.Add(InternalConditionParameter.DesignLuxLevel, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_DesignLuxLevel");

            //Space
            mapCluster.Add(SpaceParameter.Area, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_Area");
            mapCluster.Add(SpaceParameter.Volume, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_Volume");
            mapCluster.Add(SpaceParameter.FacingExternal, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_FacingExternal");
            mapCluster.Add(SpaceParameter.FacingExternalGlazing, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_FacingExternalGlazing");
            mapCluster.Add(typeof(Space), typeof(Autodesk.Revit.DB.Mechanical.Space), "Name", "SAM_SpaceName");

            //Panel
            mapCluster.Add(PanelParameter.Transparent, typeof(HostObject), "SAM_BuildingElementTransparent");
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_NorthAngle"); //double
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_FacingExternal"); //double
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_FacingExternalGlazing"); //double

            //Material
            mapCluster.Add(typeof(Core.Material), typeof(FamilyInstance), "ThermalConductivity", "SAM_Material_Conductivity");
            mapCluster.Add(typeof(Core.Material), typeof(FamilyInstance), "SpecificHeatCapacity", "SAM_Material_SpecificHeat");
            mapCluster.Add(typeof(Core.Material), typeof(FamilyInstance), "Density", "SAM_Material_Density");
            mapCluster.Add(typeof(Core.Material), typeof(FamilyInstance), "Description", "SAM_Material_Description");
            mapCluster.Add(typeof(Core.Material), typeof(FamilyInstance), "Name", "SAM_Material_Name");
            mapCluster.Add(MaterialParameter.TypeName, typeof(FamilyInstance), "SAM_Material_Type");
            mapCluster.Add(MaterialParameter.DefaultThickness, typeof(FamilyInstance), "SAM_Material_Width");
            mapCluster.Add(MaterialParameter.VapourDiffusionFactor, typeof(FamilyInstance), "SAM_Material_VapourDiffusionFactor");

            //GasMaterial
            mapCluster.Add(GasMaterialParameter.HeatTransferCoefficient, typeof(FamilyInstance), "SAM_Material_ConvectionCoefficient");

            //TransparentMaterial
            mapCluster.Add(TransparentMaterialParameter.ExternalEmissivity, typeof(FamilyInstance), "SAM_Material_ExternalEmissivity");
            mapCluster.Add(TransparentMaterialParameter.ExternalLightReflectance, typeof(FamilyInstance), "SAM_Material_ExternalLightReflectance");
            mapCluster.Add(TransparentMaterialParameter.ExternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_ExternalSolarReflectance");
            mapCluster.Add(TransparentMaterialParameter.InternalEmissivity, typeof(FamilyInstance), "SAM_Material_InternalEmissivity");
            mapCluster.Add(TransparentMaterialParameter.InternalLightReflectance, typeof(FamilyInstance), "SAM_Material_InternalLightReflectance");
            mapCluster.Add(TransparentMaterialParameter.InternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_InternalSolarReflectance");
            mapCluster.Add(TransparentMaterialParameter.IsBlind, typeof(FamilyInstance), "SAM_Material_IsBlind");
            mapCluster.Add(TransparentMaterialParameter.LightTransmittance, typeof(FamilyInstance), "SAM_Material_LightTransmittance");
            mapCluster.Add(TransparentMaterialParameter.SolarTransmittance, typeof(FamilyInstance), "SAM_Material_SolarTransmittance");

            //OpaqueMaterial
            mapCluster.Add(OpaqueMaterialParameter.ExternalEmissivity, typeof(FamilyInstance), "SAM_Material_ExternalEmissivity");
            mapCluster.Add(OpaqueMaterialParameter.ExternalLightReflectance, typeof(FamilyInstance), "SAM_Material_ExternalLightReflectance");
            mapCluster.Add(OpaqueMaterialParameter.ExternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_ExternalSolarReflectance");
            mapCluster.Add(OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations, typeof(FamilyInstance), "SAM_Material_IngnoreInUvalue");
            mapCluster.Add(OpaqueMaterialParameter.InternalEmissivity, typeof(FamilyInstance), "SAM_Material_InternalEmissivity");
            mapCluster.Add(OpaqueMaterialParameter.InternalLightReflectance, typeof(FamilyInstance), "SAM_Material_InternalLightReflectance");
            mapCluster.Add(OpaqueMaterialParameter.InternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_InternalSolarReflectance");


            result.Add(Core.Revit.ActiveSetting.Name.ParameterMap, mapCluster);

            //File Names
            result.Add(Name.FileName_DefaultAirConstructionLibrary, "SAM_AirConstructionLibrary.JSON");

            string path;

            path = Query.DefaultAirConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultAirConstructionLibrary, Core.Create.IJSAMObject<ConstructionLibrary>(System.IO.File.ReadAllText(path)));

            return result;
        }
    }
}