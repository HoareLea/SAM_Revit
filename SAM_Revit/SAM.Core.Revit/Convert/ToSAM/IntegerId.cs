// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static LongId ToSAM(this ElementId elementId)
        {
            if (elementId == null)
                return null;

            return new LongId(elementId.Value);
            
        }
    }
}