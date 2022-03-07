using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;

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
    }
}
