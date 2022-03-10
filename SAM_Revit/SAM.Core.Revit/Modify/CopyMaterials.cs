using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static IEnumerable<ElementId> CopyMaterials(this Document document, string path, IEnumerable<string> materialNames, CopyPasteOptions copyPasteOptions = null)
        {
            Func<Document, FilteredElementCollector> function = new Func<Document, FilteredElementCollector>((Document document_Temp) =>
            {
                if (document_Temp == null)
                {
                    return null;
                }

                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document_Temp).OfCategory(BuiltInCategory.OST_Materials);

                IEnumerable<Element> elements = filteredElementCollector.ToElements();
                if(elements == null)
                {
                    return null;
                }

                List<ElementId> elementIds = new List<ElementId>();
                foreach (Element element in elements)
                {
                    Autodesk.Revit.DB.Material material = element as Autodesk.Revit.DB.Material;
                    if(!materialNames.Contains(material.Name))
                    {
                        continue;
                    }

                    elementIds.Add(element.Id);
                }

                if (elementIds == null || elementIds.Count == 0)
                {
                    return null;
                }

                return new FilteredElementCollector(document_Temp, elementIds).OfCategory(BuiltInCategory.OST_Materials);
            });

            return CopyElements(document, path, function, copyPasteOptions);
        }

    }
}