using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static ElementType DuplicateByName(this Document document, Construction construction_Old, PanelType panelType, string name_New)
        {
            if (construction_Old == null || document == null || string.IsNullOrWhiteSpace(name_New))
                return null;

            BuiltInCategory builtInCategory = panelType.BuiltInCategory();
            if (builtInCategory == BuiltInCategory.INVALID)
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
                return null;

            ElementType elementType = elementTypes.Find(x => x.Name.Equals(name_New));
            if (elementType == null)
            {
                ElementType elementType_ToBeDuplicated = elementTypes.Find(x => x.Name.Equals(construction_Old.Name));
                if (elementType_ToBeDuplicated == null)
                    return null;

                elementType = elementType_ToBeDuplicated.Duplicate(name_New);
            }

            if (elementType == null)
                return null;

            Core.Revit.Modify.SetValues(elementType, construction_Old);
            Core.Revit.Modify.SetValues(elementType, construction_Old, ActiveSetting.Setting);

            return elementType;
        }
    }
}