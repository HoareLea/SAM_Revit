using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;


namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static ElementType DuplicateByConstruction(this Document document, string name_Old, PanelType panelType, Construction construction_New)
        {
            if (construction_New == null || document == null || string.IsNullOrWhiteSpace(name_Old))
                return null;

            BuiltInCategory builtInCategory = panelType.BuiltInCategory();
            if (builtInCategory == BuiltInCategory.INVALID)
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
                return null;

            ElementType elementType = elementTypes.Find(x => x.Name.Equals(construction_New.Name));
            if(elementType == null)
            {
                ElementType elementType_ToBeDuplicated = elementTypes.Find(x => x.Name.Equals(name_Old));
                if (elementType_ToBeDuplicated == null)
                    return null;

                elementType = elementType_ToBeDuplicated.Duplicate(construction_New.Name);
            }

            if (elementType == null)
                return null;

            Core.Revit.Modify.Values(construction_New, elementType);
            Core.Revit.Modify.Values(ActiveSetting.Setting, construction_New, elementType);

            return elementType;
        }
    }
}
