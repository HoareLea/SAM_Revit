// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Reflection;

namespace SAM.Core.Revit
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
            public const string ParameterMap = "Parameter Map";
            public const string ParameterName_Simplified = "ParameterName_Simplified";
            public const string ParameterName_Json = "ParameterName_Json";
        }

        private static Setting setting = null;

        private static Setting Load()
        {
            Setting setting = ActiveManager.GetSetting(Assembly.GetExecutingAssembly());
            if (setting == null)
            {
                setting = GetDefault();
            }

            return setting;
        }

        public static Setting Setting
        {
            get
            {
                if(setting == null)
                {
                    setting = Load();
                }

                return setting;
            }
        }

        public static Setting GetDefault()
        {
            Setting setting = new Setting(Assembly.GetExecutingAssembly());

            setting.Add(Name.ParameterName_Simplified, "SAM_IsNotValidEditable");
            setting.Add(Name.ParameterName_Json, "SAM_JSON");

            return setting;
        }
    }
}