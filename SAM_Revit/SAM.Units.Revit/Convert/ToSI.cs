// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Units.Revit
{
    public static partial class Convert
    {
        public static double ToSI(this double value, ForgeTypeId specTypeId)
        {
            if (specTypeId == SpecTypeId.Number)
                return value;

            ForgeTypeId unitTypeId = Query.UnitTypeId(specTypeId);
            if (unitTypeId == null)
                return value;

            return UnitUtils.ConvertFromInternalUnits(value, unitTypeId);
        }
    }
}