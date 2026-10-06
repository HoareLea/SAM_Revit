// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public class ElementBindingData : IJSAMObject
    {
        private string name;
        private HashSet<BuiltInCategory> builtInCategories;
        private ForgeTypeId groupTypeId;
        private bool instance;

        public ElementBindingData(JsonObject jObject)
        {
            FromJsonObject(jObject);
        }

        public ElementBindingData(ElementBindingData elementBindingData)
        {
            if (elementBindingData != null)
            {
                name = elementBindingData.name;
                if (elementBindingData.builtInCategories != null)
                {
                    builtInCategories = new HashSet<BuiltInCategory>();
                    foreach (BuiltInCategory builtInCategory in elementBindingData.builtInCategories)
                    {
                        builtInCategories.Add(builtInCategory);
                    }
                }

                groupTypeId = elementBindingData.groupTypeId;
                instance = elementBindingData.instance;
            }
        }

        public ElementBindingData(string name, IEnumerable<BuiltInCategory> builtInCategories, ForgeTypeId groupTypeId, bool instance)
        {
            this.name = name;
            if (builtInCategories != null)
            {
                this.builtInCategories = new HashSet<BuiltInCategory>();
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    this.builtInCategories.Add(builtInCategory);
                }
            }

            this.groupTypeId = groupTypeId;
            this.instance = instance;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public HashSet<BuiltInCategory> BuiltInCategories
        {
            get
            {
                if (builtInCategories == null)
                {
                    return null;
                }

                HashSet<BuiltInCategory> result = new HashSet<BuiltInCategory>();
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    result.Add(builtInCategory);
                }

                return result;
            }
        }

        public ForgeTypeId GroupTypeId
        {
            get
            {
                return groupTypeId;
            }
        }

        public bool Instance
        {
            get
            {
                return instance;
            }
        }

        public bool FromJsonObject(JsonObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Name"))
            {
                name = jObject["Name"]?.GetValue<string>() ?? null;
            }

            if (jObject.ContainsKey("BuiltInCategories"))
            {
                JsonArray jArray = jObject["BuiltInCategories"] as JsonArray;
                if (jArray != null)
                {
                    builtInCategories = new HashSet<BuiltInCategory>();
                    foreach (string value in jArray)
                    {
                        if (!Enum.TryParse(value, out BuiltInCategory builtInCategory))
                        {
                            continue;
                        }

                        builtInCategories.Add(builtInCategory);
                    }
                }
            }

            if (jObject.ContainsKey("GroupTypeName"))
            {
                groupTypeId = Query.GroupTypeId(jObject["GroupTypeName"]?.GetValue<string>() ?? null);
            }

            if (jObject.ContainsKey("Instance"))
            {
                instance = jObject["Instance"]?.GetValue<bool>() ?? default(bool);
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject result = new JsonObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (name != null)
            {
                result.Add("Name", name);
            }

            if (builtInCategories != null)
            {
                JsonArray jArray = new JsonArray();
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    jArray.Add(builtInCategory.ToString());
                }
                result.Add("BuiltInCategories", jArray);
            }

            if(groupTypeId != null)
            {
                result.Add("GroupTypeName", LabelUtils.GetLabelForGroup(groupTypeId));
            }

            result.Add("Instance", instance);

            return result;
        }

    }





}
