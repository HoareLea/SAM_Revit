using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static IEnumerable<ElementId> CopyElements(this Document document, string path, Func<Document, FilteredElementCollector> function, CopyPasteOptions copyPasteOptions = null)
        {
            if (document == null || string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path) || function == null)
            {
                return null;
            }

            Document document_source = null;

            try
            {
                document_source = new Autodesk.Revit.UI.UIDocument(document).Application.Application.OpenDocumentFile(path);
            }
            catch (Exception exception)
            {
                if (document_source != null)
                {
                    document_source.Close(false);
                }

                return null;
            }

            IEnumerable<ElementId> result = null;

            if (document_source != null)
            {
                FilteredElementCollector filteredElementCollector = function.Invoke(document_source);
                List<ElementId> elementIds = filteredElementCollector?.ToElementIds()?.ToList();
                if (elementIds != null && elementIds.Count != 0)
                {
                    if (copyPasteOptions == null)
                    {
                        copyPasteOptions = new CopyPasteOptions();
                        copyPasteOptions.SetDuplicateTypeNamesHandler(new DestinationDuplicateTypeNamesHandler());
                    }

                    result = ElementTransformUtils.CopyElements(document_source, elementIds, document, null, copyPasteOptions);
                }

            }

            document_source.Close(false);

            return result;

        }

    }
}