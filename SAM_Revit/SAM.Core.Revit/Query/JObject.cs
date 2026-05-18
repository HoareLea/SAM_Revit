// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using System.Text.Json.Nodes;
namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static JsonNode JsonNode(this Element element)
        {
            if (element == null)
                return null;

            string json = element.Json();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return System.Text.Json.Nodes.JsonNode.Parse(json);
        }
    }
}
