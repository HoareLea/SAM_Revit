using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static bool Placed(this Tag tag, Document document)
        {
            if (tag == null)
            {
                return false;
            }

            BuiltInCategory? builtInCategory = tag.BuiltInCategory();
            if (builtInCategory == null || !builtInCategory.HasValue || builtInCategory.Value == BuiltInCategory.INVALID)
            {
                return false;
            }

            FamilySymbol familySymbol = Core.Revit.Query.Element<FamilySymbol>(document, tag.Type, true);
            if (familySymbol == null)
            {
                return false;
            }

            Core.LongId longId_View = tag.ViewId;
            if (longId_View == null)
            {
                return false;
            }

            View view = Core.Revit.Query.Element<View>(document, longId_View, true);
            if (view == null)
            {
                return false;
            }

            Core.LongId longId_Reference = tag.ReferenceId;
            if (longId_Reference == null)
            {
                return false;
            }

            Element element = Find<Element>(document, longId_Reference);
            if (element == null)
            {
                return false;
            }
#if Revit2017
            IList<ElementId> elementIds = null;
#else
            IList<ElementId> elementIds = element.GetDependentElements(new LogicalAndFilter(new ElementCategoryFilter(builtInCategory.Value), new ElementOwnerViewFilter(view.Id)));
#endif

            if (elementIds == null || elementIds.Count == 0)
            {
                return false;
            }

            foreach (ElementId elementId in elementIds)
            {
                ElementId elementId_Type = document.GetElement(elementId)?.GetTypeId();
                if (familySymbol.Id == elementId_Type)
                {
                    return true;
                }
            }

            return false;
        }
    }
}