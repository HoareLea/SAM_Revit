using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Element Element(this Document document, LinkElementId linkElementId)
        {
            if (document == null || linkElementId == null)
                return null;

            Document revitDocument = null;
            if (linkElementId.LinkInstanceId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                revitDocument = (document.GetElement(linkElementId.LinkInstanceId) as RevitLinkInstance).GetLinkDocument();
            else
                revitDocument = document;

            if (linkElementId.LinkedElementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                return revitDocument.GetElement(linkElementId.LinkedElementId);
            else
                return revitDocument.GetElement(linkElementId.HostElementId);
        }

        public static Element Element(this Document document, string uniqueId, string linkUniqueId = null)
        {
            if (document == null)
                return null;

            Document revitDocument = null;

            if (!string.IsNullOrEmpty(linkUniqueId))
            {
                RevitLinkInstance revitLinkInstance = document.GetElement(linkUniqueId) as RevitLinkInstance;
                if (revitLinkInstance != null)
                    revitDocument = revitLinkInstance.GetLinkDocument();
            }
            else
            {
                revitDocument = document;
            }

            if (revitDocument == null)
                return null;

            return revitDocument.GetElement(uniqueId);
        }

        public static T Element<T>(this Document document, ElementId elementId) where T : Element
        {
            if (document == null || elementId == null || elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return default(T);

            return document.GetElement(elementId) as T;
        }

        public static Element Element(this EnergyAnalysisOpening energyAnalysisOpening)
        {
            if (energyAnalysisOpening == null)
            {
                return null;
            }

            ElementId elementID = ElementId(energyAnalysisOpening.OriginatingElementDescription);
            if (elementID == null || elementID == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return energyAnalysisOpening.Document.GetElement(elementID);
        }

        public static T Element<T>(this Document document, IParameterizedSAMObject parameterizedSAMObject, bool includeName = false) where T : Element
        {
            if (document == null || parameterizedSAMObject == null)
            {
                return null;
            }

            T result = null;
            if (parameterizedSAMObject.TryGetValue(ElementParameter.RevitId, out LongId longId) && longId != null)
            {
                result = Element<T>(document, longId, includeName);
            }

            if(result == null && includeName)
            {

            }

            return result;
        }

        public static T Element<T>(this Document document, LongId longId, bool includeName = false) where T : Element
        {
            if (longId == null || document == null)
            {
                return null;
            }

            T result = null;

            ElementId elementId = longId.ElementId();
            if (elementId != null && elementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                result = document.GetElement(elementId) as T;

            if (result == null)
            {
                string uniqueId = longId.UniqueId();
                if (!string.IsNullOrWhiteSpace(uniqueId))
                    result = document.GetElement(uniqueId) as T;
            }

            if (result == null && includeName)
            {
                if (longId.TryGetValue(RevitIdParameter.FullName, out string fullName) && !string.IsNullOrWhiteSpace(fullName))
                {
                    BuiltInCategory? builtInCategory = longId.BuiltInCategory();
                    if (builtInCategory != null && builtInCategory.HasValue && builtInCategory.Value != Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    {
                        List<Element> elements = new FilteredElementCollector(document).OfCategory(builtInCategory.Value).ToList();
                        if (elements != null)
                        {
                            foreach (Element element in elements)
                            {
                                T t = element as T;
                                if (t == null)
                                {
                                    continue;
                                }

                                string fullName_Element = FullName(t);
                                if (fullName.Equals(fullName_Element))
                                {
                                    return t;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}