// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static ApertureType ApertureType(this Element element)
        {

            switch ((BuiltInCategory)element.Category.Id.Value)
            {
                case Autodesk.Revit.DB.BuiltInCategory.OST_Windows:
                case Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels:
                    return Analytical.ApertureType.Window;

                case Autodesk.Revit.DB.BuiltInCategory.OST_Doors:
                    return Analytical.ApertureType.Door;
            }

            return Analytical.ApertureType.Undefined;
        }
    }
}