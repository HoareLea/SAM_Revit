// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using System.Text.Json.Nodes;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class RevitType3D : RevitType
    {
        public RevitType3D(RevitType3D revitType3D)
            :base(revitType3D)
        {

        }

        public RevitType3D(string name)
            : base(name)
        {

        }

        public RevitType3D(JsonObject jObject)
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
