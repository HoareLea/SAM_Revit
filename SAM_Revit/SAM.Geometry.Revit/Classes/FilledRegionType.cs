// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using System.Text.Json.Nodes;
namespace SAM.Geometry.Revit
{
    public class FilledRegionType : Core.Revit.RevitType
    {
        public FilledRegionType(FilledRegionType filledRegionType) 
            : base(filledRegionType)
        {
        }

        public FilledRegionType(JsonObject jObject)
            : base(jObject)
        {
        }

        public FilledRegionType(string name)
            : base(name)
        {
        }
    }
}
