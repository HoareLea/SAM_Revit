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

            //public const string Construction_Undefined = "Construction_Undefined";
            //public const string ParameterName_PanelType = "ParameterName_PanelType";
            //public const string ParameterName_ApertureHeight = "ParameterName_ApertureHeight";
            //public const string ParameterName_ApertureWidth = "ParameterName_BuildingElementWidth";
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

            //result.Add(Name.ParameterName_PanelType, "SAM_BuildingElementType");
            //result.Add(Name.ParameterName_ApertureHeight, "SAM_BuildingElementHeight");
            //result.Add(Name.ParameterName_ApertureWidth, "SAM_BuildingElementWidth");

            MapCluster mapCluster = new MapCluster();

            //Aperture
            //Instance Parameters
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), "GetWidth", "SAM_BuildingElementWidth");
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), "GetHeight", "SAM_BuildingElementHeight");
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), "Guid", "_Filter Comments 03");  //to be repalced for SAM_GUID
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), null, "_Filter Comments 01");
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), null, "_Filter Comments 02");
            mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), null, "_Filter Comments 03");
            //mapCluster.Add(typeof(Aperture), typeof(FamilyInstance), "IsRectangular", "SAM_IsNotValidEditable");  //bool
            //Type Parameters
            mapCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), null, "SAM_BuildingElementAir"); //bool
            mapCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), null, "SAM_BuildingElementColor"); //string or color
            mapCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), null, "SAM_BuildingElementDesciription"); //string
            mapCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), null, "SAM_BuildingElementGround"); //bool
            mapCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), null, "SAM_BuildingElementInternalShadows"); //bool
            mapCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), null, "SAM_BuildingElementTransparent"); //bool
            
            mapCluster.Add(ApertureConstructionParameter.DefaultPanelType, typeof(FamilySymbol), "SAM_BuildingElementType");
            mapCluster.Add(ApertureConstructionParameter.Transparent, typeof(FamilySymbol), "SAM_BuildingElementTransparent");

            mapCluster.Add(ConstructionParameter.DefaultPanelType, typeof(FamilySymbol), "SAM_BuildingElementType");
            mapCluster.Add(ConstructionParameter.Description, typeof(FamilySymbol), "SAM_BuildingElementDescription");

            mapCluster.Add(InternalConditionParameter.NumberOfPeople, typeof(Space), "SAM_NoPeople");
            mapCluster.Add(InternalConditionParameter.OccupancyProfileName, typeof(Space), "SAM_OccupacyProfile");
            mapCluster.Add(InternalConditionParameter.OccupantSensibleGain, typeof(Space), "SAM_OccupantSensGain");
            mapCluster.Add(InternalConditionParameter.OccupantLatentGain, typeof(Space), "SAM_OccupantLatGain");
            mapCluster.Add(InternalConditionParameter.EquipmentSensibleProfileName, typeof(Space), "SAM_SmallPowerSensProfile");
            mapCluster.Add(InternalConditionParameter.EquipmentLatentProfileName, typeof(Space), "SAM_SmallPowerLatProfile");
            mapCluster.Add(InternalConditionParameter.LightingGain, typeof(Space), "SAM_GenLightingGain");
            mapCluster.Add(InternalConditionParameter.DesignLuxLevel, typeof(Space), "SAM_DesignLuxLevel");

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