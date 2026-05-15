// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using System.Text.Json.Nodes;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class RevitType2D : RevitType
    {
        public RevitType2D(RevitType2D revitType2D)
            :base(revitType2D)
        {

        }

        public RevitType2D(string name)
            : base(name)
        {

        }

        public RevitType2D(JsonObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJsonObject(JsonObject jObject)
        {
            if (!base.FromJsonObject(jObject))
                return false;

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jObject = base.ToJsonObject();
            if (jObject == null)
                return jObject;

            return jObject;
        }
    }
}
