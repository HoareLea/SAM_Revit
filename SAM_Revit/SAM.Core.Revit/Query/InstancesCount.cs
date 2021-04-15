using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static int InstancesCount(this ElementType elementType)
        {
            Document document = elementType?.Document;
            if (document == null)
            {
                return -1;
            }

            if(elementType is FamilySymbol)
            {
                return new FilteredElementCollector(document).WherePasses(new FamilyInstanceFilter(document, elementType.Id)).GetElementCount();
            }

            IList<Element> elements = new FilteredElementCollector(document).WhereElementIsNotElementType().OfCategoryId(elementType.Category.Id).ToElements();
            if(elements == null)
            {
                return -1;
            }

            ElementId elementId = elementType.Id;

            int count = 0;
            foreach(Element element in elements)
            {
                if(elementId == element?.GetTypeId())
                {
                    count++;
                }
            }

            return count;
        }
    }
}