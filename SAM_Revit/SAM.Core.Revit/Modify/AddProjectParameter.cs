using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool AddProjectParameter(this Document document, ExternalDefinitionCreationOptions externalDefinitionCreationOptions, IEnumerable<BuiltInCategory> builtInCategories, bool instance, BuiltInParameterGroup builtInParameterGroup)
        {
            if(document == null || externalDefinitionCreationOptions == null || builtInCategories == null)
            {
                return false;
            }

            string name = externalDefinitionCreationOptions.Name;
            if(string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            CategorySet categorySet = Create.CategorySet(document, builtInCategories);
            if(categorySet == null || categorySet.Size == 0)
            {
                return false;
            }

            ElementBinding elementBinding = null;

            if(Query.TryGetElementBinding(document, externalDefinitionCreationOptions.Name, out InternalDefinition internalDefinition, out elementBinding))
            {
                return false;
            }

            Autodesk.Revit.ApplicationServices.Application application = new UIDocument(document).Application.Application;

            bool result = false;

            string path = System.IO.Path.GetTempFileName();
            try
            {
                using (SharedParameterFileWrapper sharedParameterFileWrapper = new SharedParameterFileWrapper(application))
                {
                    Definition definition = sharedParameterFileWrapper.Create(System.IO.Path.GetRandomFileName(), externalDefinitionCreationOptions);
                    if(definition != null)
                    {
                        if (instance)
                        {
                            elementBinding = application.Create.NewInstanceBinding(categorySet);
                        }
                        else
                        {
                            elementBinding = application.Create.NewTypeBinding(categorySet);
                        }

                        if(elementBinding != null)
                        {
                            result = document.ParameterBindings.Insert(definition, elementBinding, builtInParameterGroup);
                        }
                    }
                }
            }
            catch(Exception exception)
            {

            }
            finally
            {
                if(System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            return result;
        }
    }
}