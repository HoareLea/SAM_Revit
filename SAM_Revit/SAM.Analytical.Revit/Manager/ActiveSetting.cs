﻿using System.Reflection;

using Autodesk.Revit.DB;

using SAM.Core;


namespace SAM.Analytical.Revit
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            //public const string Construction_Undefined = "Construction_Undefined";
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

            SAMRelationCluster sAMRelationCluster = new SAMRelationCluster();

            //Aperture
            //Instance Parameters
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("GetWidth", "SAM_BuildingElementWidth"));
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("GetHeight", "SAM_BuildingElementHeight"));
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("Guid", "_Filter Comments 03"));  //to be repalced for SAM_GUID
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation(null, "_Filter Comments 01"));
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation(null, "_Filter Comments 02"));
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation(null, "_Filter Comments 03"));
            //sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("IsRectangular", "SAM_IsNotValidEditable"));  //bool
            //Type Parameters
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementAir")); //bool
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementColor")); //string or color
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementDesciription")); //string
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementGround")); //bool
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementInternalShadows")); //bool
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementTransparent")); //bool

            result.Add(Core.Revit.ActiveSetting.Name.ParameterMap, sAMRelationCluster);

            return result;
        }
    }
}