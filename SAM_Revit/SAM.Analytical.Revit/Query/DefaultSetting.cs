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
            sAMRelationCluster.Add(typeof(Aperture), typeof(FamilyInstance), new SAMRelation("", ""));

            result.Add("Parameter Map", sAMRelationCluster);
            return result;
        }
    }
}
