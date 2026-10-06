// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Architectural.Revit
{
    public static partial class Query
    {
        public static double Elevation(this Autodesk.Revit.DB.Level level)
        {
            if (level == null)
                return double.NaN;

            return UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters);
        }
    }
}