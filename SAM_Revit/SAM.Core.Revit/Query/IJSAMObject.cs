// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using System.Text.Json.Nodes;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<T> IJSAMObjects<T>(this Element element) where T : IJSAMObject
        {
            if (element == null)
                return default;

            JsonNode jToken = element.JsonNode();
            if (jToken == null)
                return default;

            switch(jToken.GetValueKind())
            {
                case System.Text.Json.JsonValueKind.Object:
                    T t = Core.Create.IJSAMObject<T>(jToken as JsonObject);
                    if(t != null)
                    {
                        return new List<T>() { t };
                    }
                    break;

                case System.Text.Json.JsonValueKind.Array:
                    return Core.Create.IJSAMObjects<T>(jToken as JsonArray);
            }

            return null;
        }
    }
}
