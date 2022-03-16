using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAM.Core.Revit
{
    public class SharedParameterFileWrapper : IDisposable
    {
        private Autodesk.Revit.ApplicationServices.Application application;
        private string path;

        private DefinitionFile definitionFile;

        public SharedParameterFileWrapper(Autodesk.Revit.ApplicationServices.Application application)
        {
            this.application = application;
            if (application != null)
            {
                try
                {
                    path = application.SharedParametersFilename;
                }
                catch
                {
                    path = string.Empty;
                }
            }
        }

        public void New(string path)
        {
            definitionFile = null;

            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllText(path, string.Empty);

            if (application != null)
            {
                application.SharedParametersFilename = path;
                definitionFile = application.OpenSharedParameterFile();
            }
        }

        public void Open(string Path)
        {
            definitionFile = null;

            if (application != null)
            {
                application.SharedParametersFilename = Path;
                definitionFile = application.OpenSharedParameterFile();
            }
        }

        public void Open()
        {
            definitionFile = null;

            if (string.IsNullOrEmpty(path))
                return;

            if (File.Exists(path))
            {
                application.SharedParametersFilename = path;
                definitionFile = application.OpenSharedParameterFile();
            }
        }

        public HashSet<string> GetNames()
        {
            if (definitionFile?.Groups == null)
            {
                return null;
            }

            HashSet<string> result = new HashSet<string>();

            foreach (DefinitionGroup definitionGroup in definitionFile.Groups)
            {
                if (definitionGroup.Definitions == null)
                {
                    continue;
                }

                foreach (Definition definition in definitionGroup.Definitions)
                {
                    result.Add(definition.Name);
                }
            }

            return result;
        }

        public List<Definition> FindAll(params string[] names)
        {
            if (definitionFile == null)
                return null;

            List<Definition> definitions = new List<Definition>();

            if (definitionFile.Groups == null)
                return definitions;

            foreach (DefinitionGroup definitionGroup in definitionFile.Groups)
            {
                if (definitionGroup.Definitions == null)
                    continue;

                foreach (Definition definition in definitionGroup.Definitions)
                    foreach (string name in names)
                        if (definition.Name == name)
                        {
                            definitions.Add(definition);
                            break;
                        }
            }

            return definitions;
        }

        public Definition Find(string name)
        {
            if (definitionFile?.Groups == null)
                return null;

            foreach (DefinitionGroup definitionGroup in definitionFile.Groups)
            {
                if (definitionGroup.Definitions == null)
                {
                    continue;
                }

                foreach (Definition definition in definitionGroup.Definitions)
                {
                    if (definition?.Name == name)
                    {
                        return definition;
                    }
                }
            }

            return null;
        }

        public Definition Create(string group, ExternalDefinitionCreationOptions externalDefinitionCreationOptions)
        {
            if (definitionFile == null)
                return null;

            DefinitionGroup definitionGroup = null;
            foreach (DefinitionGroup definitionGroup_Temp in definitionFile.Groups)
            {
                if (definitionGroup_Temp.Name == group)
                {
                    definitionGroup = definitionGroup_Temp;
                    break;
                }
            }

            if (definitionGroup == null)
                definitionGroup = definitionFile.Groups.Create(group);

            Definition definition = definitionGroup.Definitions.Create(externalDefinitionCreationOptions);
            return definition;
        }

        public ElementBinding Add(Document document, string name, IEnumerable<BuiltInCategory> builtInCategories, BuiltInParameterGroup builtInParameterGroup, bool instance)
        {
            if (document == null || document.ParameterBindings == null || string.IsNullOrWhiteSpace(name) || builtInCategories == null || builtInCategories.Count() == 0)
            {
                return null;
            }

            ElementBinding result = null;

            Definition definition = null;

            DefinitionBindingMapIterator definitionBindingMapIterator = document?.ParameterBindings?.ForwardIterator();
            if(definitionBindingMapIterator != null)
            {
                definitionBindingMapIterator.Reset();

                while (definitionBindingMapIterator.MoveNext())
                {
                    definition = definitionBindingMapIterator.Key as Definition;
                    if (definition == null)
                        continue;

                    //if (aExternalDefinition.GUID.Equals(pExternalDefinition.GUID))
                    if (definition.Name.Equals(name))
                    {
                        result = (ElementBinding)definitionBindingMapIterator.Current;
                        break;
                    }
                }
            }

            if(result != null)
            {
                return result;
            }

            if(definition == null)
            {
                return null;
            }

            CategorySet categorySet = Revit.Create.CategorySet(document, builtInCategories);
            if (categorySet == null || categorySet.Size == 0)
            {
                return null;
            }

            if (result != null)
            {

            }
            else
            {
                if (instance)
                    result = document.Application.Create.NewInstanceBinding(categorySet);
                else
                    result = document.Application.Create.NewTypeBinding(categorySet);

               document.ParameterBindings.Insert(definition, result, builtInParameterGroup);
            }

            return result;
        }

        public ElementBinding Add(Document document, ElementBindingData elementBindingData)
        {
            if(document == null || elementBindingData == null)
            {
                return null;
            }

            return Add(document, elementBindingData.Name, elementBindingData.BuiltInCategories, elementBindingData.BuiltInParameterGroup, elementBindingData.Instance);
        }

        public void Close()
        {
            if (application != null)
            {
                application.SharedParametersFilename = path;
                if (string.IsNullOrEmpty(path) || File.Exists(path))
                {
                    try
                    {
                        application.OpenSharedParameterFile();
                    }
                    catch (Exception aException)
                    {

                    }

                }

            }

        }

        public void Dispose()
        {
            Close();
        }

        ~SharedParameterFileWrapper() { Dispose(); }
    }
}
