using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool Simplified(this Element element)
        {
            if (element == null)
                return false;

            Setting setting = ActiveSetting.Setting;

            string name;
            if (!setting.TryGetValue(ActiveSetting.Name.ParameterName_Simplified, out name))
                return false;

            Parameter parameter = element.LookupParameter(name);
            if (parameter == null || parameter.StorageType != StorageType.Integer)
                return false;

            return parameter.AsInteger() == 1;
        }
    }
}
