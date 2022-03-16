using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public class ElementBindingData : IJSAMObject
    {
        private string name;
        private HashSet<BuiltInCategory> builtInCategories;
        private BuiltInParameterGroup builtInParameterGroup;
        private bool instance;

        public ElementBindingData(JObject jObject)
        {
            FromJObject(jObject);
        }

        public ElementBindingData(ElementBindingData elementBindingData)
        {
            if(elementBindingData != null)
            {
                name = elementBindingData.name;
                if(elementBindingData.builtInCategories != null)
                {
                    builtInCategories = new HashSet<BuiltInCategory>();
                    foreach(BuiltInCategory builtInCategory in elementBindingData.builtInCategories)
                    {
                        builtInCategories.Add(builtInCategory);
                    }
                }

                builtInParameterGroup = elementBindingData.builtInParameterGroup;
                instance = elementBindingData.instance;
            }
        }

        public ElementBindingData(string name, IEnumerable<BuiltInCategory> builtInCategories, BuiltInParameterGroup builtInParameterGroup, bool instance)
        {
            this.name = name;
            if(builtInCategories != null)
            {
                this.builtInCategories = new HashSet<BuiltInCategory>();
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    this.builtInCategories.Add(builtInCategory);
                }
            }

            this.builtInParameterGroup = builtInParameterGroup;
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
                if(builtInCategories == null)
                {
                    return null;
                }

                HashSet<BuiltInCategory> result = new HashSet<BuiltInCategory>();
                foreach(BuiltInCategory builtInCategory in builtInCategories)
                {
                    result.Add(builtInCategory);
                }

                return result;
            }
        }

        public BuiltInParameterGroup BuiltInParameterGroup
        {
            get
            {
                return builtInParameterGroup;
            }
        }

        public bool Instance
        {
            get
            {
                return instance;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Name"))
            {
                name = jObject.Value<string>("Name");
            }

            if(jObject.ContainsKey("BuiltInCategories"))
            {
                JArray jArray = jObject.Value<JArray>("BuiltInCategories");
                if(jArray != null)
                {
                    builtInCategories = new HashSet<BuiltInCategory>();
                    foreach(string value in jArray)
                    {
                        if(!Enum.TryParse(value, out BuiltInCategory builtInCategory))
                        {
                            continue;
                        }

                        builtInCategories.Add(builtInCategory);
                    }
                }
            }

            if(jObject.ContainsKey("BuiltInParameterGroup"))
            {
                string value = jObject.Value<string>("BuiltInParameterGroup");
                if(!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, out BuiltInParameterGroup builtInParameterGroup_Temp))
                {
                    builtInParameterGroup = builtInParameterGroup_Temp;
                }
            }

            if(jObject.ContainsKey("Instance"))
            {
                instance = jObject.Value<bool>("Instance");
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (name != null)
            {
                result.Add("Name", name);
            }

            if(builtInCategories != null)
            {
                JArray jArray = new JArray();
                foreach(BuiltInCategory builtInCategory in builtInCategories)
                {
                    jArray.Add(builtInCategory.ToString());
                }
                result.Add("BuiltInCategories", jArray);
            }

            result.Add("BuiltInParameterGroup", builtInParameterGroup.ToString());

            result.Add("Instance", instance);
            
            return result;
        }

    }
}
