// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using System.Diagnostics;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Process StartProcess(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return Core.Query.StartProcess(path);
        }
    }
}