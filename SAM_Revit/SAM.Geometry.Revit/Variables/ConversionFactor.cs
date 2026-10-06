// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Units.Revit
{
    public static class ConversionFactor
    {

        public static double FromFeetToMeter { get; } = UnitUtils.ConvertFromInternalUnits(1, UnitTypeId.Meters);
        public static double FromMeterToFeet { get; } = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Meters);
    }
}