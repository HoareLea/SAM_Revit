// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Text.Json.Nodes;
namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool SetJsonArray(this SAMSchema sAMSchema, Element element, JsonArray jArray)
        {
            if (sAMSchema == null || element == null || jArray == null)
                return false;

            string fieldName = sAMSchema.FieldName;
            if (string.IsNullOrWhiteSpace(fieldName))
                return false;

            return SetJsonArray(sAMSchema.GetSchema(), element, jArray, fieldName);
        }

        public static bool SetJsonArray(this Schema schema, Element element, JsonArray jArray, string fieldName)
        {
            if (schema == null || element == null || jArray == null)
                return false;

            Entity entity = new Entity(schema);
            if (entity == null)
                return false;

            Field field = schema.GetField(fieldName);
            if (field == null)
                return false;

            entity.Set(field, jArray.ToString());
            element.SetEntity(entity);
            return true;
        }
    }
}
