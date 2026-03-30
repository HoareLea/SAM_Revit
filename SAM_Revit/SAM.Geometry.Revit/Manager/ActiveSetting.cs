using SAM.Core;
// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Reflection;

namespace SAM.Geometry.Revit
{
    public static partial class ActiveSetting
    {
        public static class Name
        {
        }

        private static Setting setting = null;

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

            return setting;
        }
    }
}