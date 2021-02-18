using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static System.Guid DocumentGuid(this Document document)
        {
            if (document?.IsValidObject != true)
                return System.Guid.Empty;

            return ExportUtils.GetGBXMLDocumentId(document);
        }
    }
}