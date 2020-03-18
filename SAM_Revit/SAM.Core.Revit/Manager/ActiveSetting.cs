using System.Reflection;


namespace SAM.Core.Revit
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            public const string ParameterMap = "Parameter Map";
            public const string ParameterName_Simplified = "ParameterName_Simplified";
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
            Setting setting = new Setting(Assembly.GetExecutingAssembly());

            setting.Add(Name.ParameterName_Simplified, "SAM_IsNotValidEditable");

            return setting;
        }
    }
}
