namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool SetJson(this Autodesk.Revit.DB.Element element, string json)
        {
            if (element == null)
                return false;

            Setting setting = ActiveSetting.Setting;

            string name;
            if (!setting.TryGetValue(ActiveSetting.Name.ParameterName_Json, out name))
                return false;

            return SetValue(element.LookupParameter(name), json);
        }
    }
}