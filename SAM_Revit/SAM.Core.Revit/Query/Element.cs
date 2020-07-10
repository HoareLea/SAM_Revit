using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

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

        public static T Element<T>(this Document document, ElementId elementId) where T: Element
        {
            if (document == null || elementId == null || elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return default(T);

            return document.GetElement(elementId) as T;
        }

        public static Element Element(this EnergyAnalysisOpening energyAnalysisOpening)
        {
            ElementId elementID = Query.ElementId(energyAnalysisOpening.OriginatingElementDescription);
            if (elementID == null || elementID == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return energyAnalysisOpening.Document.GetElement(elementID);
        }
    }
}