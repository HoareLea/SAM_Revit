// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024

#else
using System.Runtime.InteropServices.Marshalling;
#endif

namespace SAM.Core.Revit
{
    public class ConvertSettings : IJSAMObject
    {
        private bool convertGeometry;
        private bool convertParameters;
        private bool removeExisting;
        private bool useProjectLocation;

        private Dictionary<string, List<object>> objects;

        private Dictionary<string, object> parameters;

        public ConvertSettings(bool convertGeometry, bool convertParameters, bool removeExisting, bool useProjectLocation = false)
        {
            objects = new Dictionary<string, List<object>>();
            parameters = null;

            this.convertGeometry = convertGeometry;
            this.convertParameters = convertParameters;
            this.removeExisting = removeExisting;
            this.useProjectLocation = useProjectLocation;
        }

        public ConvertSettings(ConvertSettings convertSettings)
        {
            convertGeometry = convertSettings.convertGeometry;
            convertParameters = convertSettings.convertParameters;
            removeExisting = convertSettings.removeExisting;
            useProjectLocation = convertSettings.useProjectLocation;

            if(convertSettings.parameters != null)
            {
                parameters = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> keyValuePair in convertSettings.parameters)
                    parameters[keyValuePair.Key] = keyValuePair.Value;
            }

            objects = new Dictionary<string, List<object>>();
        }

        public ConvertSettings(JsonObject jObject)
        {
            FromJsonObject(jObject);
        }

        public bool ConvertGeometry
        {
            get
            {
                return convertGeometry;
            }
        }

        public bool ConvertParameters
        {
            get
            {
                return convertParameters;
            }
        }

        public bool RemoveExisting
        {
            get
            {
                return removeExisting;
            }
        }

        public bool UseProjectLocation
        {
            get
            {
                return useProjectLocation;
            }
        }

        public Dictionary<string, object> GetParameters()
        {
            if (parameters == null)
                return null;

            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> keyValuePair in parameters)
                result[keyValuePair.Key] = keyValuePair.Value;

            return result;
        }

        public bool AddParameter(string name, object value)
        {
            if (name == null)
                return false;

            if (parameters == null)
                parameters = new Dictionary<string, object>();

            parameters[name] = value;
            return true;
        }

        public bool FromJsonObject(JsonObject jObject)
        {
            if (jObject == null)
                return false;

            convertGeometry = jObject["ConvertGeometry"]?.GetValue<bool>() ?? default(bool);
            convertParameters = jObject["ConvertParameters"]?.GetValue<bool>() ?? default(bool);
            removeExisting = jObject["RemoveExisting"]?.GetValue<bool>() ?? default(bool);
            useProjectLocation = jObject["UseProjectLocation"]?.GetValue<bool>() ?? default(bool);
            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject jObject = new JsonObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
            jObject.Add("ConvertGeometry", convertGeometry);
            jObject.Add("ConvertParameters", convertParameters);
            jObject.Add("RemoveExisting", removeExisting);
            jObject.Add("UseProjectLocation", useProjectLocation);

            return jObject;
        }

        public bool Add(System.Guid guid, Element element)
        {
            if (guid == System.Guid.Empty)
                return false;

            objects[guid.ToString()] = new List<object> { element };
            return true;
        }

        public bool Add<T>(System.Guid guid, IEnumerable<T> elements) where T: Element
        {
            if (guid == System.Guid.Empty)
                return false;

            objects[guid.ToString()] = elements?.Cast<object>().ToList();
            return true;
        }

        public bool Add(ElementId elementId, ISAMObject sAMObject)
        {
            if (elementId == null || elementId == ElementId.InvalidElementId)
                return false;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            objects[elementId.IntegerValue.ToString()] = new List<object> { sAMObject };
#else
            objects[elementId.Value.ToString()] = new List<object> { sAMObject };
#endif

            return true;
        }

        public bool Add<T>(ElementId elementId, IEnumerable<T> sAMObjects) where T: IJSAMObject
        {
            if (elementId == null || elementId == ElementId.InvalidElementId)
                return false;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            objects[elementId.IntegerValue.ToString()] = sAMObjects?.Cast<object>().ToList();
#else
            objects[elementId.Value.ToString()] = sAMObjects?.Cast<object>().ToList();
#endif

            return true;
        }

        public T GetObject<T>(System.Guid guid) where T: Element
        {
            List<object> objects = GetObjects(guid.ToString());
            if (objects != null && objects.Count != 0)
            {
                T @object = objects[0] as T;
                if (@object == null || !@object.IsValidObject)
                    return null;

                return objects[0] as T;
            }
                
            return  null;
        }

        public T GetObject<T>(ElementId elementId) where T : ISAMObject
        {

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            List<object> objects = GetObjects(elementId.IntegerValue.ToString());
#else
            List<object> objects = GetObjects(elementId.Value.ToString());
#endif
            if (objects == null || objects.Count == 0)
                return default;

            object result = objects[0];

            if (result is T)
                return (T)result;

            return default;
        }

        public List<T> GetObjects<T>(ElementId elementId) where T : ISAMObject
        {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            List<object> objects = GetObjects(elementId.IntegerValue.ToString());
#else
            List<object> objects = GetObjects(elementId.Value.ToString());
#endif

            if (objects == null)
                return null;

            if (objects.Count == 0)
                return new List<T>();

            return objects.FindAll(x => x is T).ConvertAll(x => (T)x);
        }

        public List<T> GetObjects<T>(System.Guid guid) where T : Element
        {
            List<object> objects = GetObjects(guid.ToString());
            if (objects == null)
                return null;

            if (objects.Count == 0)
                return new List<T>();

            return objects.FindAll(x => x is T).ConvertAll(x => (T)x);
        }

        public bool Contains(System.Guid guid)
        {
            return objects.ContainsKey(guid.ToString());
        }

        public bool Contains(ElementId elementId)
        {
            if (elementId == null)
                return false;
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return objects.ContainsKey(elementId.IntegerValue.ToString());
#else
            return objects.ContainsKey(elementId.Value.ToString());
#endif
        }

        public bool ClearObjects()
        {
            if (objects == null || objects.Count == 0)
                return false;

            objects.Clear();
            return true;
        }

        private List<object> GetObjects(string id)
        {
            if (id == null)
                return null;

            List<object> objects = null;
            if (!this.objects.TryGetValue(id, out objects))
                return null;

            return objects;
        }
    }
}