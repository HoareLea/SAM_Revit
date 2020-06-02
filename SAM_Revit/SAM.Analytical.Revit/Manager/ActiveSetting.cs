using Autodesk.Revit.DB;
using SAM.Core;
using System.Reflection;

namespace SAM.Analytical.Revit
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            //public const string Construction_Undefined = "Construction_Undefined";
            public const string ParameterName_PanelType = "ParameterName_PanelType";
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

            result.Add(Core.Revit.ActiveSetting.Name.ParameterMap, mapCluster);

            result.Add(Name.ParameterName_PanelType, "SAM_BuildingElementType");

            return result;
        }
    }
}