using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static IEnumerable<ElementId> CopyElementTypes(this Document document, string path, IEnumerable<string> fullNames, BuiltInCategory? builtInCategory = null, CopyPasteOptions copyPasteOptions = null)
        {

            Func<Document, FilteredElementCollector> function = new Func<Document, FilteredElementCollector>((Document document_Temp) =>
            {
                if (document_Temp == null)
                {
                    return null;
                }

                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document_Temp).WhereElementIsElementType();
                if (builtInCategory != null && builtInCategory.HasValue && builtInCategory.Value != BuiltInCategory.INVALID)
                {
                    filteredElementCollector.OfCategory(builtInCategory.Value);
                }

                List<ElementId> elementIds = new List<ElementId>();
                foreach (Element element in filteredElementCollector)
                {
                    string fullName_Source = Query.FullName(element);
                    if (string.IsNullOrWhiteSpace(fullName_Source))
                    {
                        continue;
                    }

                    if (!fullNames.Contains(fullName_Source))
                    {
                        continue;
                    }

                    elementIds.Add(element.Id);
                }

                if(elementIds == null || elementIds.Count == 0)
                {
                    return null;
                }

                return new FilteredElementCollector(document_Temp, elementIds);
            });

            return CopyElements(document, path, function, copyPasteOptions);
        }
    }
}