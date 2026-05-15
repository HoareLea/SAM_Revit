// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using System.Text.Json.Nodes;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class TagType : RevitType
    {
        public TagType(TagType tagType)
            : base(tagType)
        {
        }

        public TagType(JsonObject jObject)
            : base(jObject)
        {
        }

        public TagType(string name)
            : base(name)
        {
        }
    }
}
