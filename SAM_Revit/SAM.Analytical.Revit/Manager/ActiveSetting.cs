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
            public const string ParameterMap_Cooling = "Parameter Map Cooling";
            public const string ParameterMap_Heating = "Parameter Map Heating";
            public const string ParameterMap_Ventilation = "Parameter Map Ventilation";

            public const string ParameterMap_Tools = "Parameter Map Tools";
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

            TypeMap parameterMap_General = Core.Create.TypeMap();

            //AnalyticalModel
            parameterMap_General.Add(AnalyticalModelParameter.NorthAngle, typeof(ProjectInfo), "SAM_NorthAngle");
            parameterMap_General.Add(AnalyticalModelParameter.CoolingSizingFactor, typeof(ProjectInfo), "SAM_SizingFactorCooling");
            parameterMap_General.Add(AnalyticalModelParameter.HeatingSizingFactor, typeof(ProjectInfo), "SAM_SizingFactorHeating");

            //Aperture
            parameterMap_General.Add(typeof(Aperture), typeof(FamilyInstance), "GetWidth", "SAM_BuildingElementWidth");
            parameterMap_General.Add(typeof(Aperture), typeof(FamilyInstance), "GetHeight", "SAM_BuildingElementHeight");
            parameterMap_General.Add(typeof(Aperture), typeof(FamilyInstance), "Guid", "SAM_GUID");

            //ApertureConstruction
            parameterMap_General.Add(ApertureConstructionParameter.Color, typeof(FamilySymbol), "SAM_BuildingElementColor");
            parameterMap_General.Add(ApertureConstructionParameter.Transparent, typeof(FamilySymbol), "SAM_BuildingElementTransparent");
            parameterMap_General.Add(ApertureConstructionParameter.Description, typeof(FamilySymbol), "SAM_BuildingElementDescription");
            parameterMap_General.Add(ApertureConstructionParameter.DefaultFrameWidth, typeof(FamilySymbol), "SAM_BuildingElementFrameWidth");
            parameterMap_General.Add(ApertureConstructionParameter.IsInternalShadow, typeof(FamilySymbol), "SAM_BuildingElementInternalShadows");
            //MD 2021-02-25 this property will duplicate from default type
            //parameterMap_General.Add(typeof(ApertureConstruction), typeof(FamilySymbol), "ApertureType", "SAM_BuildingElementType");

            //Construction
            parameterMap_General.Add(ConstructionParameter.Color, typeof(HostObjAttributes), "SAM_BuildingElementColor");
            parameterMap_General.Add(ConstructionParameter.DefaultPanelType, typeof(HostObjAttributes), "SAM_BuildingElementType");
            parameterMap_General.Add(ConstructionParameter.Description, typeof(HostObjAttributes), "SAM_BuildingElementDescription");
            parameterMap_General.Add(ConstructionParameter.IsAir, typeof(HostObjAttributes), "SAM_BuildingElementAir");
            parameterMap_General.Add(ConstructionParameter.IsInternalShadow, typeof(HostObjAttributes), "SAM_BuildingElementInternalShadows");
            parameterMap_General.Add(ConstructionParameter.IsGround, typeof(HostObjAttributes), "SAM_BuildingElementGround");
            parameterMap_General.Add(ConstructionParameter.Transparent, typeof(HostObjAttributes), "SAM_BuildingElementTransparent");
            parameterMap_General.Add(ConstructionParameter.DefaultThickness, typeof(HostObjAttributes), "SAM_BuildingElementThickness");

            //InternalCondition
            parameterMap_General.Add(typeof(InternalCondition), typeof(Autodesk.Revit.DB.Mechanical.Space), "Name", "SAM_IC_ThermalTemplate");


            parameterMap_General.Add(InternalConditionParameter.VentilationSystemTypeName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_VentType");
            parameterMap_General.Add(InternalConditionParameter.HeatingSystemTypeName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_HeatingTypePrimary");
            parameterMap_General.Add(InternalConditionParameter.CoolingSystemTypeName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_CoolingTypePrimary");

            parameterMap_General.Add(InternalConditionParameter.OccupancyProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupacyProfile");
            parameterMap_General.Add(InternalConditionParameter.OccupancySensibleGainPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantSensGain");
            parameterMap_General.Add(InternalConditionParameter.OccupancyLatentGainPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantLatGain");
            parameterMap_General.Add(InternalConditionParameter.AreaPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupantDensity");

            parameterMap_General.Add(InternalConditionParameter.EquipmentSensibleGainPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpecSmallPowerSens");
            parameterMap_General.Add(InternalConditionParameter.EquipmentSensibleProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SmallPowerSensProfile");
            parameterMap_General.Add(InternalConditionParameter.EquipmentLatentGainPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpecSmallPowerLat");
            parameterMap_General.Add(InternalConditionParameter.EquipmentLatentProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SmallPowerLatProfile");
            
            parameterMap_General.Add(InternalConditionParameter.LightingGainPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpecGenLighting");
            parameterMap_General.Add(InternalConditionParameter.LightingProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_GenLightingProfile");
            parameterMap_General.Add(InternalConditionParameter.LightingLevel, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_DesignLuxLevel");

            parameterMap_General.Add(InternalConditionParameter.InfiltrationAirChangesPerHour, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_CLGInfiltrationACH");
            parameterMap_General.Add(InternalConditionParameter.InfiltrationProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_InfiltrationProfile");
            
            parameterMap_General.Add(InternalConditionParameter.PollutantProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_PollutantProfile");
            parameterMap_General.Add(InternalConditionParameter.PollutantGenerationPerArea, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_PollutantGeneration_ghrm2");
            parameterMap_General.Add(InternalConditionParameter.PollutantGenerationPerPerson, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_PollutantGeneration_ghrperson");
            
            parameterMap_General.Add(InternalConditionParameter.HeatingEmitterRadiantProportion, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_HeatingEmmiterRadiantProportion"); // Add formula to copy value from System if InternalCondition has no value
            parameterMap_General.Add(InternalConditionParameter.HeatingEmitterCoefficient, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_HeatingEmmiterCoefficient"); // Add formula to copy value from System if InternalCondition has no value
            parameterMap_General.Add(InternalConditionParameter.HeatingProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceHeatingProfile");
            
            parameterMap_General.Add(InternalConditionParameter.CoolingEmitterRadiantProportion, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_CoolingEmmiterRadiantProportion"); // Add formula to copy value from System if InternalCondition has no value
            parameterMap_General.Add(InternalConditionParameter.CoolingEmitterCoefficient, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_CoolingEmmiterCoefficient"); // Add formula to copy value from System if InternalCondition has no value
            parameterMap_General.Add(InternalConditionParameter.CoolingProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceCoolingProfile");
            
            parameterMap_General.Add(InternalConditionParameter.HumidificationProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceHumidificationProfile");
            parameterMap_General.Add(InternalConditionParameter.DehumidificationProfileName, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_SpaceDehumidificationProfile");

            //Space
            parameterMap_General.Add(SpaceParameter.Area, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_Area");
            parameterMap_General.Add(SpaceParameter.Occupancy, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_NoPeople", null, "[SAM.Analytical.Query.CalculatedOccupancy(Object_1)]");
            parameterMap_General.Add(SpaceParameter.Volume, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_Volume");
            parameterMap_General.Add(SpaceParameter.FacingExternal, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_FacingExternal");
            parameterMap_General.Add(SpaceParameter.FacingExternalGlazing, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_FacingExternalGlazing");
            parameterMap_General.Add(SpaceParameter.CoolingSizingFactor, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OversizingFactorCooling");
            parameterMap_General.Add(SpaceParameter.HeatingSizingFactor, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OversizingFactorHeating");
            parameterMap_General.Add(typeof(Space), typeof(Autodesk.Revit.DB.Mechanical.Space), "Name", "SAM_SpaceName");
            parameterMap_General.Add(typeof(Space), typeof(Autodesk.Revit.DB.Mechanical.Space), "HeatingDesignTemperature", "SAM_WinterDesignTemperature", null, "[SAM.Analytical.Query.HeatingDesignTemperature(Object_1, AnalyticalModel)]");
            parameterMap_General.Add(typeof(Space), typeof(Autodesk.Revit.DB.Mechanical.Space), "CoolingDesignTemperature", "SAM_SummerDesignTemperature", null, "[SAM.Analytical.Query.CoolingDesignTemperature(Object_1, AnalyticalModel)]");
            parameterMap_General.Add(typeof(Space), typeof(Autodesk.Revit.DB.Mechanical.Space), "SpecificOccupancySensibleGain", "SAM_SpecOccupancySens", null, "[SAM.Analytical.Query.SpecificOccupancySensibleGain(Object_1)]");
            parameterMap_General.Add(SpaceParameter.SupplyAirFlow, typeof(Autodesk.Revit.DB.Mechanical.Space), "Specified Supply Airflow");
            parameterMap_General.Add(SpaceParameter.ExhaustAirFlow, typeof(Autodesk.Revit.DB.Mechanical.Space), "Specified Exhaust Airflow");
            parameterMap_General.Add(SpaceParameter.OutsideSupplyAirFlow, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_TotalZoneOutsideSupplyAirFlow");

            //SpaceSimulationResultParameter
            parameterMap_General.Add(SpaceSimulationResultParameter.Area, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_Area'");
            parameterMap_General.Add(SpaceSimulationResultParameter.Volume, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_Volume'");
            parameterMap_General.Add(SpaceSimulationResultParameter.OccupiedHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_OccupancyNoHoursYearly");
            parameterMap_General.Add(SpaceSimulationResultParameter.OccupiedHours25, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv25'");
            parameterMap_General.Add(SpaceSimulationResultParameter.OccupiedHours28, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv28'");
            parameterMap_General.Add(SpaceSimulationResultParameter.OccupiedHours25, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv25Perc'", null, "[SAM.Analytical.Revit.Query.OccupiedHours25Percentage(Object_1)]");
            parameterMap_General.Add(SpaceSimulationResultParameter.OccupiedHours28, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ComfNoHoursAbv28Perc'", null, "[SAM.Analytical.Revit.Query.OccupiedHours28Percentage(Object_1)]");
            parameterMap_General.Add(SpaceSimulationResultParameter.MaxDryBulbTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceDryBulbTemperature_Max'");
            parameterMap_General.Add(SpaceSimulationResultParameter.MinDryBulbTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceDryBulbTemperature_Min'");
            parameterMap_General.Add(SpaceSimulationResultParameter.MaxDryBulbTemperatureIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_Date_SpaceDryBulbTemperature_Max'", null, "[SAM.Analytical.Convert.ToString(Value)]");
            parameterMap_General.Add(SpaceSimulationResultParameter.MinDryBulbTemperatureIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_Date_SpaceDryBulbTemperature_Min'", null, "[SAM.Analytical.Convert.ToString(Value)]");
            parameterMap_General.Add(typeof(SpaceSimulationResult), typeof(Autodesk.Revit.DB.Mechanical.Space), "DateTime", "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ResultsImportTime'", null, "[SAM.Analytical.Convert.ToString(Value)]");
            parameterMap_General.Add(typeof(SpaceSimulationResult), typeof(Autodesk.Revit.DB.Mechanical.Space), "Reference", "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceGUID'");

            //Panel
            parameterMap_General.Add(PanelParameter.Transparent, typeof(HostObject), "SAM_BuildingElementTransparent");
            parameterMap_General.Add(PanelParameter.ThermalTransmittance, typeof(HostObject), "SAM_SrfUValue");
            parameterMap_General.Add(PanelParameter.LightTransmittance, typeof(HostObject), "SAM_SrfLightTransmittance");
            parameterMap_General.Add(PanelParameter.LightReflectance, typeof(HostObject), "SAM_SrfLightReflectance");
            parameterMap_General.Add(PanelParameter.DirectSolarEnergyTransmittance, typeof(HostObject), "SAM_SrfDirectSolarEnergyTransmittance");
            parameterMap_General.Add(PanelParameter.DirectSolarEnergyReflectance, typeof(HostObject), "SAM_SrfDirectSolarEnergyReflectance");
            parameterMap_General.Add(PanelParameter.DirectSolarEnergyAbsorptance, typeof(HostObject), "SAM_SrfDirectSolarEnergyAbsorptance");
            parameterMap_General.Add(PanelParameter.TotalSolarEnergyTransmittance, typeof(HostObject), "SAM_SrfgValue");
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_NorthAngle"); //double

            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_FacingExternal"); //double
            //mapCluster.Add(typeof(Panel), typeof(HostObject), null, "SAM_FacingExternalGlazing"); //double

            //Aperture
            parameterMap_General.Add(ApertureParameter.ThermalTransmittance, typeof(FamilyInstance), "SAM_SrfUValue");
            parameterMap_General.Add(ApertureParameter.LightTransmittance, typeof(FamilyInstance), "SAM_SrfLightTransmittance");
            parameterMap_General.Add(ApertureParameter.LightReflectance, typeof(FamilyInstance), "SAM_SrfLightReflectance");
            parameterMap_General.Add(ApertureParameter.DirectSolarEnergyTransmittance, typeof(FamilyInstance), "SAM_SrfDirectSolarEnergyTransmittance");
            parameterMap_General.Add(ApertureParameter.DirectSolarEnergyReflectance, typeof(FamilyInstance), "SAM_SrfDirectSolarEnergyReflectance");
            parameterMap_General.Add(ApertureParameter.DirectSolarEnergyAbsorptance, typeof(FamilyInstance), "SAM_SrfDirectSolarEnergyAbsorptance");
            parameterMap_General.Add(ApertureParameter.TotalSolarEnergyTransmittance, typeof(FamilyInstance), "SAM_SrfgValue");

            //Material
            parameterMap_General.Add(typeof(Core.Material), typeof(FamilyInstance), "ThermalConductivity", "SAM_Material_Conductivity");
            parameterMap_General.Add(typeof(Core.Material), typeof(FamilyInstance), "SpecificHeatCapacity", "SAM_Material_SpecificHeat");
            parameterMap_General.Add(typeof(Core.Material), typeof(FamilyInstance), "Density", "SAM_Material_Density");
            parameterMap_General.Add(typeof(Core.Material), typeof(FamilyInstance), "Description", "SAM_Material_Description");
            parameterMap_General.Add(typeof(Core.Material), typeof(FamilyInstance), "Name", "SAM_Material_Name");
            parameterMap_General.Add(RevitMaterialParameter.TypeName, typeof(FamilyInstance), "SAM_Material_Type");
            parameterMap_General.Add(Core.MaterialParameter.DefaultThickness, typeof(FamilyInstance), "SAM_Material_Width");
            parameterMap_General.Add(MaterialParameter.VapourDiffusionFactor, typeof(FamilyInstance), "SAM_Material_VapourDiffusionFactor");

            //GasMaterial
            parameterMap_General.Add(GasMaterialParameter.HeatTransferCoefficient, typeof(FamilyInstance), "SAM_Material_ConvectionCoefficient");

            //TransparentMaterial
            parameterMap_General.Add(TransparentMaterialParameter.ExternalEmissivity, typeof(FamilyInstance), "SAM_Material_ExternalEmissivity");
            parameterMap_General.Add(TransparentMaterialParameter.ExternalLightReflectance, typeof(FamilyInstance), "SAM_Material_ExternalLightReflectance");
            parameterMap_General.Add(TransparentMaterialParameter.ExternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_ExternalSolarReflectance");
            parameterMap_General.Add(TransparentMaterialParameter.InternalEmissivity, typeof(FamilyInstance), "SAM_Material_InternalEmissivity");
            parameterMap_General.Add(TransparentMaterialParameter.InternalLightReflectance, typeof(FamilyInstance), "SAM_Material_InternalLightReflectance");
            parameterMap_General.Add(TransparentMaterialParameter.InternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_InternalSolarReflectance");
            parameterMap_General.Add(TransparentMaterialParameter.IsBlind, typeof(FamilyInstance), "SAM_Material_IsBlind");
            parameterMap_General.Add(TransparentMaterialParameter.LightTransmittance, typeof(FamilyInstance), "SAM_Material_LightTransmittance");
            parameterMap_General.Add(TransparentMaterialParameter.SolarTransmittance, typeof(FamilyInstance), "SAM_Material_SolarTransmittance");

            //OpaqueMaterial
            parameterMap_General.Add(OpaqueMaterialParameter.ExternalEmissivity, typeof(FamilyInstance), "SAM_Material_ExternalEmissivity");
            parameterMap_General.Add(OpaqueMaterialParameter.ExternalLightReflectance, typeof(FamilyInstance), "SAM_Material_ExternalLightReflectance");
            parameterMap_General.Add(OpaqueMaterialParameter.ExternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_ExternalSolarReflectance");
            parameterMap_General.Add(OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations, typeof(FamilyInstance), "SAM_Material_IngnoreInUvalue");
            parameterMap_General.Add(OpaqueMaterialParameter.InternalEmissivity, typeof(FamilyInstance), "SAM_Material_InternalEmissivity");
            parameterMap_General.Add(OpaqueMaterialParameter.InternalLightReflectance, typeof(FamilyInstance), "SAM_Material_InternalLightReflectance");
            parameterMap_General.Add(OpaqueMaterialParameter.InternalSolarReflectance, typeof(FamilyInstance), "SAM_Material_InternalSolarReflectance");


            result.Add(Core.Revit.ActiveSetting.Name.ParameterMap, parameterMap_General);

            TextMap textMap_Zone = Core.Create.TextMap("Zone Map");
            textMap_Zone.Add(ZoneType.Cooling.Text(), "SAM_ZoneCoolingReference");
            textMap_Zone.Add(ZoneType.Heating.Text(), "SAM_ZoneHeatingReference");
            textMap_Zone.Add(ZoneType.Ventilation.Text(), "SAM_ZoneVentilationReference");
            result.Add(Name.ZoneMap, textMap_Zone);

            TypeMap parameterMap_Cooling = Core.Create.TypeMap();
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.UnmetHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursCooling");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.UnmetHourFirstIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHourCoolingFirstInst");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.OccupiedHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursCoolingOccupied");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.SizingMethod, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SizingMethod'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_PeakDate'", null, "[SAM.Analytical.Convert.ToString(Value)]");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_PeakTime'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.DryBulbTempearture, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceDryBulbTemperature'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.ResultantTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceResultantTemperature'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.Load, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_CoolingLoad'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.SolarGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SolarGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.LightingGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_LightingGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.InfiltrationGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_InfVentGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.AirMovementGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_AirMovementGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.BuildingHeatTransfer, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_BuildingHeatTransfer'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.GlazingExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ExternalConductionGlazing'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.OpaqueExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_ExternalConductionOpaque'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.OccupancySensibleGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_OccupancySensibleGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.EquipmentSensibleGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_EquipmentSensibleGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.EquipmentLatentGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_EquipmentLatentGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.OccupancyLatentGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_OccupancyLatentGain'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.HumidityRatio, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceHumidityRatio'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.RelativeHumidity, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceRelativeHumidity'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.ApertureFlowIn, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceAperutreFlowIn'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.ApertureFlowOut, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpaceAperutreFlowOut'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.Pollutant, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_SpacePollutant'");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.DesignLoad, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_DesSpecSensCoolLoad'", null, "[SAM.Analytical.Query.SpecificDesignLoad(Object_1)]");
            parameterMap_Cooling.Add(SpaceSimulationResultParameter.DesignLoad, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_DesSensCoolingLoad'");// "[SAM.Analytical.Query.SpecificLoad(Object_1)]");
            parameterMap_Cooling.Add(AdjacencyClusterSimulationResultParameter.UnmetHours, typeof(ProjectInfo), "SAM_BuildingUnmetHoursCooling");
            parameterMap_Cooling.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneOutsideSupplyAirFlow", "SAM_ZoneCLGTotalOutsideSupplyAirFlow", null, "[SAM.Analytical.Query.CalculatedOutsideSupplyAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Cooling.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneSupplyAirFlow", "SAM_ZoneCLGSpecifiedSupplyAirflow", null, "[SAM.Analytical.Query.CalculatedSupplyAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Cooling.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneExhaustAirFlow", "SAM_ZoneCLGSpecifiedExhaustAirflow", null, "[SAM.Analytical.Query.CalculatedExhaustAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Cooling.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "HeatingSensibleLoad", "SAM_ZoneCLGHeatingSensLoad", null, "[SAM.Analytical.Query.DesignHeatingLoad(AdjacencyCluster, Object_1)]");
            parameterMap_Cooling.Add(ZoneSimulationResultParameter.MaxSensibleLoad, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_ZoneCLGCoolingSensLoad");
            parameterMap_Cooling.Add(ZoneSimulationResultParameter.MaxSensibleLoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_ZoneCLGCoolingSensLoadPeakDate", null, "[SAM.Analytical.Convert.ToString(Value)]");
            result.Add(Name.ParameterMap_Cooling, parameterMap_Cooling);

            TypeMap parameterMap_Heating= Core.Create.TypeMap();
            parameterMap_Heating.Add(SpaceSimulationResultParameter.UnmetHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursHeating");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.UnmetHourFirstIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHourHeatingFirstInst");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.OccupiedHours, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_UnmetHoursHeatingOccupied");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.SizingMethod, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SizingMethod'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_PeakDate'", null, "[SAM.Analytical.Convert.ToString(Value)]");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.LoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_PeakTime'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.DryBulbTempearture, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SpaceDryBulbTemperature'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.ResultantTemperature, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SpaceResultantTemperature'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.Load, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HeatingLoad'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.InfiltrationGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_InfVentGain'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.AirMovementGain, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_AirMovementGain'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.GlazingExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_ExternalConductionGlazing'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.OpaqueExternalConduction, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_ExternalConductionOpaque'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.HumidityRatio, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_SpaceHumidityRatio'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.BuildingHeatTransfer, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_HDD_BuildingHeatTransfer'");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.DesignLoad, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_DesSpecSensHeatLoad'", null, "[SAM.Analytical.Query.SpecificDesignLoad(Object_1)]");
            parameterMap_Heating.Add(SpaceSimulationResultParameter.DesignLoad, typeof(Autodesk.Revit.DB.Mechanical.Space), "='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_DesignSensHeatingLoad'");//"='SAM' + [SAM.Core.Revit.Query.ParameterNamePrefix(Object_1)] + '_Height'", null, "[SAM.Analytical.Query.Height(Object_1)]");
            parameterMap_Heating.Add(AdjacencyClusterSimulationResultParameter.UnmetHours, typeof(ProjectInfo), "SAM_BuildingUnmetHoursHeating");
            parameterMap_Heating.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneOutsideSupplyAirFlow", "SAM_ZoneHTGTotalOutsideSupplyAirFlow", null, "[SAM.Analytical.Query.CalculatedOutsideSupplyAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Heating.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneSupplyAirFlow", "SAM_ZoneHTGSpecifiedSupplyAirflow", null, "[SAM.Analytical.Query.CalculatedSupplyAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Heating.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneExhaustAirFlow", "SAM_ZoneHTGSpecifiedExhaustAirflow", null, "[SAM.Analytical.Query.CalculatedExhaustAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Heating.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "HeatingSensibleLoad", "SAM_ZoneHTGHeatingSensLoad", null, "[SAM.Analytical.Query.DesignHeatingLoad(AdjacencyCluster, Object_1)]");
            parameterMap_Heating.Add(ZoneSimulationResultParameter.MaxSensibleLoad, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_ZoneHTGCoolingSensLoad");
            parameterMap_Heating.Add(ZoneSimulationResultParameter.MaxSensibleLoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_ZoneHTGCoolingSensLoadPeakDate", null, "[SAM.Analytical.Convert.ToString(Value)]");
            result.Add(Name.ParameterMap_Heating, parameterMap_Heating);

            TypeMap parameterMap_Ventilation = Core.Create.TypeMap();
            parameterMap_Ventilation.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneOutsideSupplyAirFlow", "SAM_ZoneVNTTotalOutsideSupplyAirFlow", null, "[SAM.Analytical.Query.CalculatedOutsideSupplyAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Ventilation.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneSupplyAirFlow", "SAM_ZoneVNTSpecifiedSupplyAirFlow", null, "[SAM.Analytical.Query.CalculatedSupplyAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Ventilation.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "ZoneExhaustAirFlow", "SAM_ZoneVNTSpecifiedExhaustAirFlow", null, "[SAM.Analytical.Query.CalculatedExhaustAirFlow(AdjacencyCluster, Object_1)]");
            parameterMap_Ventilation.Add(typeof(Zone), typeof(Autodesk.Revit.DB.Mechanical.Space), "HeatingSensibleLoad", "SAM_ZoneVNTHeatingSensLoad", null, "[SAM.Analytical.Query.DesignHeatingLoad(AdjacencyCluster, Object_1)]");
            parameterMap_Ventilation.Add(ZoneSimulationResultParameter.MaxSensibleLoad, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_ZoneVNTCoolingSensLoad");
            parameterMap_Ventilation.Add(ZoneSimulationResultParameter.MaxSensibleLoadIndex, typeof(Autodesk.Revit.DB.Mechanical.Space), "SAM_ZoneVNTCoolingSensLoadPeakDate", null, "[SAM.Analytical.Convert.ToString(Value)]");
            result.Add(Name.ParameterMap_Ventilation, parameterMap_Ventilation);

            //File Names
            result.Add(Name.FileName_DefaultAirConstructionLibrary, "SAM_AirConstructionLibrary.JSON");

            result.Add(Name.ParameterMap_Tools, "ParameterMap_Tools");

            string path;

            path = Query.DefaultAirConstructionLibraryPath(result);
            if (System.IO.File.Exists(path))
                result.Add(Name.Library_DefaultAirConstructionLibrary, Core.Create.IJSAMObject<ConstructionLibrary>(System.IO.File.ReadAllText(path)));

            return result;
        }
    }
}