namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool SetSimplified(this Autodesk.Revit.DB.Element element, bool simplified)
        {
            if (element == null)
                return false;

            Setting setting = ActiveSetting.Setting;

            string name;
            if (!setting.TryGetValue(ActiveSetting.Name.ParameterName_Simplified, out name))
                return false;

            return SetValue(element.LookupParameter(name), simplified);
        }
    }
}