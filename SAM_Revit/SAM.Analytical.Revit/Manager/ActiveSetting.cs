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
            public const string ZoneMap = "Zone Map";
            public const string ResultCoolingMap = "Result Cooling Map";
            public const string ResultHeatingMap = "Result Heating Map";
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

            TypeMap typeMap = new TypeMap();

            //AnalyticalModel
            typeMap.Add(AnalyticalModelParameter.NorthAngle, typeof(ProjectInfo), "SAM_NorthAngle");
            typeMap.Add(AnalyticalModelParameter.CoolingSizingFactor, typeof(ProjectInfo), "SAM_SizingFactorCooling");
            typeMap.Add(AnalyticalModelParameter.HeatingSizingFactor, typeof(ProjectInfo), "SAM_SizingFactorHeating");

            //Aperture
            typeMap.Add(typeof(Aperture), typeof(FamilyInstance), "GetWidth", "SAM_BuildingElementWidth");
            typeMap.Add(typeof(Aperture), typeof(FamilyInstance), "GetHeight", "SAM_BuildingElementHeight");
            typeMap.Add(typeof(Aperture), typeof(FamilyInstance), "Guid", "SAM_GUID");

            //ApertureConstruction
            typeMap.Add(ApertureConstructionParameter.Color, typeof(FamilySymbol), "SAM_BuildingElementColor");
            typeMap.Add(ApertureConstructionParameter.DefaultPanelType, typeof(FamilySymbol), "SAM_BuildingElementType");
            typeMap.Add(ApertureConstructionParameter.Transparent, typeof(FamilySymbol), "SAM_BuildingElementTransparent");
            typeMap.Add(ApertureConstructionParameter.Description, typeof(FamilySymbol), "SAM_BuildingElementDescription");
            typeMap.Add(ApertureConstructionParameter.DefaultFrameWidth, typeof(FamilySymbol), "SAM_BuildingElementFrameWidth");
            typeMap.Add(ApertureConstructionParameter.IsInternalShadow, typeof(FamilySymbol), "SAM_BuildingElementInternalShadows");

            //Construction
            typeMap.Add(ConstructionParameter.Color, typeof(HostObjAttributes), "SAM_BuildingElementColor");
            typeMap.Add(ConstructionParameter.DefaultPanelType, typeof(HostObjAttributes), "SAM_BuildingElementType");
            typeMap.Add(ConstructionParameter.Description, typeof(HostObjAttributes), "SAM_BuildingElementDescription");
            typeMap.Add(ConstructionParameter.IsAir, typeof(HostObjAttributes), "SAM_BuildingElementAir");
            typeMap.Add(ConstructionParameter.IsInternalShadow, typeof(HostObjAttributes), "SAM_BuildingElementInternalShadows");
            typeMap.Add(ConstructionParameter.IsGround, typeof(HostObjAttributes), "SAM_BuildingElementGround");
            typeMap.Add(ConstructionParameter.Transparent, typeof(HostObjAttributes), "SAM_BuildingElementTransparent");
            typeMap.Add(ConstructionParameter.DefaultThickness, typeof(HostObjAttributes), "SAM_BuildingElementThickness");

            //InternalCondition
            typeMap.Add(typeof(InternalCondition), typeof(Autodesk.Revit.DB.Mechanical.Space), "Name", "SAM_IC_ThermalTemplate");

            typeMap.Add(InternalConditionParameter.OccupancyProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupacyProfile");
            typeMap.Add(InternalConditionParameter.OccupancySensibleGainPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantSensGain");
            typeMap.Add(InternalConditionParameter.OccupancyLatentGainPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantLatGain");
            typeMap.Add(InternalConditionParameter.AreaPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantDensity");

            typeMap.Add(InternalConditionParameter.EquipmentSensibleGainPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpecSmallPowerSens");
            typeMap.Add(InternalConditionParameter.EquipmentSensibleProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SmallPowerSensProfile");
            typeMap.Add(InternalConditionParameter.EquipmentLatentGainPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpecSmallPowerLat");
            typeMap.Add(InternalConditionParameter.EquipmentLatentProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SmallPowerLatProfile");
            
            typeMap.Add(InternalConditionParameter.LightingGainPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpecGenLighting");
            typeMap.Add(InternalConditionParameter.LightingProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_GenLightingProfile");
            typeMap.Add(InternalConditionParameter.LightingLevel, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_DesignLuxLevel");

            typeMap.Add(InternalConditionParameter.InfiltrationAirChangesPerHour, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_CLGInfiltrationACH");
            typeMap.Add(InternalConditionParameter.InfiltrationProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_InfiltrationProfile");
            
            typeMap.Add(InternalConditionParameter.PollutantProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_PollutantProfile");
            typeMap.Add(InternalConditionParameter.PollutantGenerationPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_PollutantGeneration_ghrm2");
            typeMap.Add(InternalConditionParameter.PollutantGenerationPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_PollutantGeneration_ghrperson");
            
            typeMap.Add(InternalConditionParameter.HeatingEmitterRadiantProportion, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_HeatingEmmiterRadiantProportion");
            typeMap.Add(InternalConditionParameter.HeatingEmitterCoefficient, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_HeatingEmmiterCoefficient");
            typeMap.Add(InternalConditionParameter.HeatingProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceHeatingProfile");
            
            typeMap.Add(InternalConditionParameter.CoolingEmitterRadiantProportion, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_CoolingEmmiterRadiantProportion");
            typeMap.Add(InternalConditionParameter.CoolingEmitterCoefficient, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_CoolingEmmiterCoefficient");
            typeMap.Add(InternalConditionParameter.CoolingProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceCoolingProfile");
            
            typeMap.Add(InternalConditionParameter.HumidificationProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceHumidificationProfile");
            typeMap.Add(InternalConditionParameter.DehumidificationProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceDehumidificationProfile");

            //Space
            typeMap.Add(SpaceParameter.Area, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_Area");
            typeMap.Add(SpaceParameter.Occupancy, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_NoPeople");
            typeMap.Add(SpaceParameter.Volume, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_Volume");
            typeMap.Add(SpaceParameter.FacingExternal, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_FacingExternal");
            typeMap.Add(SpaceParameter.FacingExternalGlazing, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_FacingExternalGlazing");
            typeMap.Add(SpaceParameter.CoolingSizingFactor, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OversizingFactorCooling");
            typeMap.Add(SpaceParameter.HeatingSizingFactor, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OversizingFactorHeating");
            typeMap.Add(typeof(Space), typeof(Autodesk.Revit.DB.Mechanical.Space), "Name", "SAM_SpaceName");

            typeMap.Add(SpaceSimulationResultParameter.OccupiedHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupancyNoHoursYearly");
            typeMap.Add(SpaceSimulationResultParameter.OccupiedHours25, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv25'");
            typeMap.Add(SpaceSimulationResultParameter.OccupiedHours28, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv28'");
            typeMap.Add(SpaceSimulationResultParameter.OccupiedHours25, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv25Perc'");
            typeMap.Add(SpaceSimulationResultParameter.OccupiedHours28, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv28Perc'");
            typeMap.Add(SpaceSimulationResultParameter.MaxDryBulbTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceDryBulbTemperature_Max'");
            typeMap.Add(SpaceSimulationResultParameter.MinDryBulbTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceDryBulbTemperature_Min'");
            typeMap.Add(SpaceSimulationResultParameter.MaxDryBulbTemperatureIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_Date_SpaceDryBulbTemperature_Max'");
            typeMap.Add(SpaceSimulationResultParameter.MinDryBulbTemperatureIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_Date_SpaceDryBulbTemperature_Min'");
            typeMap.Add(typeof(SpaceSimulationResult), typeof(Autodesk.Revit.DB.Mechanical.Space), "DateTime", "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ResultsImportTime'");

            //Panel
            typeMap.Add(PanelParameter.Transparent, typeof(HostObject), "SAM_BuildingElementTransparent");
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_NorthAngle"); //double
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_FacingExternal"); //double
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_FacingExternalGlazing"); //double

            //Material
            typeMap.Add(typeof(Core.Material), typeof(FamilyInstance), "ThermalConductivity", "SAM_Material_Conductivity");
            typeMap.Add(typeof(Core.Material), typeof(FamilyInstance), "SpecificHeatCapacity", "SAM_Material_SpecificHeat");
            typeMap.Add(typeof(Core.Material), typeof(FamilyInstance), "Density", "SAM_Material_Density");
            typeMap.Add(typeof(Core.Material), typeof(FamilyInstance), "Description", "SAM_Material_Description");
            typeMap.Add(typeof(Core.Material), typeof(FamilyInstance), "Name", "SAM_Material_Name");
            typeMap.Add(MaterialParameter.TypeName, typeof(FamilyInstance), "SAM_Material_Type");
            typeMap.Add(MaterialParameter.DefaultThickness, typeof(FamilyInstance), "SAM_Material_Width");
            typeMap.Add(MaterialParameter.VapourDiffusionFactor, typeof(FamilyInstance), "SAM_Material_VapourDiffusionFactor");

            //GasMaterial
            typeMap.Add(GasMaterialParameter.HeatTransferCoefficient, typeof(FamilyInstance), "SAM_Material_ConvectionCoefficient");

            //TransparentMaterial
            typeMap.Add(TransparentMaterialParameter.ExternalEmissivity, typeof(FamilyInstance), "SAM_Material_ExternalEmissivity");
            typeMap.Add(TransparentMaterialParameter.ExternalLightReflectance, typeof(FamilyInstance), "SAM_Material_ExternalLightReflectance");
            typeMap.Add(TransparentMaterialParameter.ExternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_ExternalSolarReflectance");
            typeMap.Add(TransparentMaterialParameter.InternalEmissivity, typeof(FamilyInstance), "SAM_Material_InternalEmissivity");
            typeMap.Add(TransparentMaterialParameter.InternalLightReflectance, typeof(FamilyInstance), "SAM_Material_InternalLightReflectance");
            typeMap.Add(TransparentMaterialParameter.InternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_InternalSolarReflectance");
            typeMap.Add(TransparentMaterialParameter.IsBlind, typeof(FamilyInstance), "SAM_Material_IsBlind");
            typeMap.Add(TransparentMaterialParameter.LightTransmittance, typeof(FamilyInstance), "SAM_Material_LightTransmittance");
            typeMap.Add(TransparentMaterialParameter.SolarTransmittance, typeof(FamilyInstance), "SAM_Material_SolarTransmittance");

            //OpaqueMaterial
            typeMap.Add(OpaqueMaterialParameter.ExternalEmissivity, typeof(FamilyInstance), "SAM_Material_ExternalEmissivity");
            typeMap.Add(OpaqueMaterialParameter.ExternalLightReflectance, typeof(FamilyInstance), "SAM_Material_ExternalLightReflectance");
            typeMap.Add(OpaqueMaterialParameter.ExternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_ExternalSolarReflectance");
            typeMap.Add(OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations, typeof(FamilyInstance), "SAM_Material_IngnoreInUvalue");
            typeMap.Add(OpaqueMaterialParameter.InternalEmissivity, typeof(FamilyInstance), "SAM_Material_InternalEmissivity");
            typeMap.Add(OpaqueMaterialParameter.InternalLightReflectance, typeof(FamilyInstance), "SAM_Material_InternalLightReflectance");
            typeMap.Add(OpaqueMaterialParameter.InternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_InternalSolarReflectance");


            result.Add(Core.Revit.ActiveSetting.Name.ParameterMap, typeMap);

            TextMap textMap_Zone = new TextMap("Zone Map");
            textMap_Zone.Add(ZoneType.Cooling.Text(), "SAM_ZoneCoolingReference");
            textMap_Zone.Add(ZoneType.Heating.Text(), "SAM_ZoneHeatingReference");
            textMap_Zone.Add(ZoneType.Ventilation.Text(), "SAM_ZoneVentilationReference");
            result.Add(Name.ZoneMap, textMap_Zone);

            TypeMap typeMap_Result_Cooling = new TypeMap();
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.UnmetHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursCooling");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.UnmetHourFirstIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHourCoolingFirstInst");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.OccupiedHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursCoolingOccupied");

            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.SizingMethod, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SizingMethod'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_PeakDate'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_PeakTime'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.DryBulbTempearture, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceDryBulbTemperature'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.ResultantTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceResultantTemperature'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.Load, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_CoolingLoad'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.SolarGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SolarGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.LightingGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_LightingGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.InfiltrationGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_InfVentGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.AirMovementGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_AirMovementGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.BuildingHeatTransfer, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_BuildingHeatTransfer'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.GlazingExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ExternalConductionGlazing'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.OpaqueExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ExternalConductionOpaque'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.OccupancySensibleGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_OccupancySensibleGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.EquipmentSensibleGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_EquipmentSensibleGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.EquipmentLatentGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_EquipmentLatentGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.OccupancyLatentGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_OccupancyLatentGain'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.HumidityRatio, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceHumidityRatio'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.RelativeHumidity, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceRelativeHumidity'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.ApertureFlowIn, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceAperutreFlowIn'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.ApertureFlowOut, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceAperutreFlowOut'");
            typeMap_Result_Cooling.Add(SpaceSimulationResultParameter.Pollutant, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpacePollutant'");
            result.Add(Name.ResultCoolingMap, typeMap_Result_Cooling);

            TypeMap typeMap_Result_Heating= new TypeMap();
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.UnmetHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursHeating");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.UnmetHourFirstIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHourHeatingFirstInst");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.OccupiedHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursHeatingOccupied");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.SizingMethod, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SizingMethod'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_PeakDate'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_PeakTime'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.DryBulbTempearture, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SpaceDryBulbTemperature'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.ResultantTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SpaceResultantTemperature'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.Load, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HeatingLoad'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.InfiltrationGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_InfVentGain'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.AirMovementGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_AirMovementGain'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.GlazingExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_ExternalConductionGlazing'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.OpaqueExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_ExternalConductionOpaque'");
            typeMap_Result_Heating.Add(SpaceSimulationResultParameter.HumidityRatio, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SpaceHumidityRatio'");
            result.Add(Name.ResultHeatingMap, typeMap_Result_Heating);

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