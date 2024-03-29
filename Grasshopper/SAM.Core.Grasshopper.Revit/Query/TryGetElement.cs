﻿using Autodesk.Revit.DB;
using Grasshopper.Kernel.Types;

namespace SAM.Core.Grasshopper.Revit
{
    public static partial class Query
    {
        public static bool TryGetElement<T>(this GH_ObjectWrapper objectWrapper,  out T element, Document document = null) where T: Element
        {
            element = null;

            if (objectWrapper == null)
                return false;
            
            dynamic obj = objectWrapper.Value;
            if (obj is RhinoInside.Revit.GH.Types.ProjectDocument)
                return false;

            if(obj is SAMObject)
            {
                if (document == null)
                    document = RhinoInside.Revit.Revit.ActiveDBDocument;

                SAMObject sAMObject = obj as SAMObject;
                string uniqueId = Core.Revit.Query.UniqueId(sAMObject);
                if(!string.IsNullOrWhiteSpace(uniqueId))
                {
                    element = document.GetElement(uniqueId) as T;
                }

                if(element == null)
                {
                    ElementId elementId = Core.Revit.Query.ElementId(sAMObject);
                    if(elementId != null && elementId != ElementId.InvalidElementId)
                    {
                        element = document.GetElement(elementId) as T;
                    }
                }
            }
            else if(obj is int)
            {
                ElementId elementId = new ElementId((int)obj);
                element = (obj.Document as Document).GetElement(elementId) as T;
            }
            else
            {
                ElementId elementId = obj.Id as ElementId;

                element = (obj.Document as Document).GetElement(elementId) as T;
            }

            return element != null;
        }
    }
}