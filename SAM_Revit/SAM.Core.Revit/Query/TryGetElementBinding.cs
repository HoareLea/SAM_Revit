using Autodesk.Revit.DB;
using System;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool TryGetElementBinding(this Document document, Guid guid, out InternalDefinition internalDefinition, out ElementBinding elementBinding)
        {
            internalDefinition = null;
            elementBinding = null;

            if (document == null || document.ParameterBindings == null)
                return false;

            DefinitionBindingMapIterator definitionBindingMapIterator = document.ParameterBindings.ForwardIterator();
            if (definitionBindingMapIterator == null)
                return false;

            while (definitionBindingMapIterator.MoveNext())
            {
                InternalDefinition internalDefinition_Temp = definitionBindingMapIterator.Key as InternalDefinition;
                if (internalDefinition_Temp == null)
                    continue;

                ElementId elementId = internalDefinition_Temp.Id;
                if (elementId == null || elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                    continue;

                SharedParameterElement sharedParameterElement = document.GetElement(elementId) as SharedParameterElement;
                if (sharedParameterElement == null)
                    continue;

                if (guid == sharedParameterElement.GuidValue)
                {
                    elementBinding = (ElementBinding)definitionBindingMapIterator.Current;
                    internalDefinition = internalDefinition_Temp;
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetElementBinding(this Document document, string name, out InternalDefinition internalDefinition, out ElementBinding elementBinding)
        {
            internalDefinition = null;
            elementBinding = null;

            if (document == null || document.ParameterBindings == null || string.IsNullOrEmpty(name))
                return false;

            DefinitionBindingMapIterator definitionBindingMapIterator = document.ParameterBindings.ForwardIterator();
            if (definitionBindingMapIterator == null)
                return false;

            while (definitionBindingMapIterator.MoveNext())
            {
                InternalDefinition aInternalDefinition_Temp = definitionBindingMapIterator.Key as InternalDefinition;
                if (aInternalDefinition_Temp == null)
                    continue;

                ElementId elementId = aInternalDefinition_Temp.Id;
                if (elementId == null || elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                    continue;

                ParameterElement parameterElement = document.GetElement(elementId) as ParameterElement;
                if (parameterElement == null)
                    continue;

                if (name == parameterElement.Name)
                {
                    elementBinding = (ElementBinding)definitionBindingMapIterator.Current;
                    internalDefinition = aInternalDefinition_Temp;
                    return true;
                }
            }
            return false;
        }
    }
}