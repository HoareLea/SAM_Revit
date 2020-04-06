using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;


namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static ElementType Duplicate(this Document document, Construction construction, PanelType panelType, string name)
        {
            if (construction == null || document == null || string.IsNullOrWhiteSpace(name))
                return null;

            BuiltInCategory builtInCategory = panelType.BuiltInCategory();
            if (builtInCategory == BuiltInCategory.INVALID)
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
                return null;

            ElementType elementType = elementTypes.Find(x => x.Name.Equals(name));
            if(elementType == null)
            {
                ElementType elementType_ToBeDuplicated = elementTypes.Find(x => x.Name.Equals(construction.Name));
                if (elementType_ToBeDuplicated == null)
                    return null;

                elementType = elementType_ToBeDuplicated.Duplicate(name);
            }

            if (elementType == null)
                return null;

            Core.Revit.Modify.Values(construction, elementType);
            Core.Revit.Modify.Values(ActiveSetting.Setting, construction, elementType);

            return elementType;
        }
    }
}
