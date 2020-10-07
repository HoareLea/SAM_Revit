namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static ConstructionLibrary DefaultAirConstructionLibrary()
        {
            ConstructionLibrary result = null;
            if (ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.Library_DefaultAirConstructionLibrary, out result))
                return result;

            return null;
        }
    }
}