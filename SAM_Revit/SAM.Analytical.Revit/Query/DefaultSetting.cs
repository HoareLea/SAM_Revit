using System.Reflection;

using Autodesk.Revit.DB;

using SAM.Core;


namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static Setting DefaultSetting()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            Setting result = new Setting(assembly);

            SAMRelationCluster sAMRelationCluster = new SAMRelationCluster();

            //Aperture
            //Instance Parameters
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("GetWidth", "SAM_BuildingElementWidth"));
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("GetHeight", "SAM_BuildingElementHeight"));
            //Type Parameters
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementAir")); //bool
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementColor")); //string or color
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementDesciription")); //string
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementGround")); //bool
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementInternalShadows")); //bool
            sAMRelationCluster.Add(typeof(ApertureConstruction), typeof(FamilySymbol), new SAMRelation(null, "SAM_BuildingElementTransparent")); //bool


            //sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("GetHeight", "SAM_BuildingElementHeight"));
            //sAMRelationCluster.Add(typeof(Panel), typeof(Wall), new SAMRelation("GetWidth", "SAM_BuildingElementWidth"));
            //sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("", ""));

            result.Add("Parameter Map", sAMRelationCluster);
            return result;
        }
    }
}
