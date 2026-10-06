// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static PanelGroup PanelGroup(this HostObjAttributes hostObjAttributes)
        {
            if (hostObjAttributes == null)
                return Analytical.PanelGroup.Undefined;
            return PanelType((BuiltInCategory)hostObjAttributes.Category.Id.Value).PanelGroup();
        }
    }
}