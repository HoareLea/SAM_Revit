using Autodesk.Revit.DB;


namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string JObject(this Element element)
        {
            if (element == null)
                return null;

            Setting setting = ActiveSetting.Setting;

            string name;
            if (!setting.TryGetValue(ActiveSetting.Name.ParameterName_Json, out name))
                return null;

            Parameter parameter = element.LookupParameter(name);
            if (parameter == null || parameter.StorageType != StorageType.String)
                return null;

            return parameter.AsString();
        }
    }
}
