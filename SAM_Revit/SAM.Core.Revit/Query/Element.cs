using Autodesk.Revit.DB;

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
    }
}