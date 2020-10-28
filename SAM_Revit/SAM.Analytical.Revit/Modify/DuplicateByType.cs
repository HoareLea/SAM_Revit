using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static ElementType DuplicateByType(this Document document, string name_Old, PanelType panelType, Construction construction_New)
        {
            if (construction_New == null || document == null || string.IsNullOrWhiteSpace(name_Old))
                return null;

            FilteredElementCollector filteredElementCollector = Query.FilteredElementCollector(document, panelType);
            if (filteredElementCollector == null)
                return null;

            List<ElementType> elementTypes = filteredElementCollector.OfClass(typeof(ElementType)).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
                return null;

            ElementType elementType = elementTypes.Find(x => x.Name.Equals(construction_New.Name));
            if (elementType == null)
            {
                ElementType elementType_ToBeDuplicated = elementTypes.Find(x => x.Name.Equals(name_Old));
                if (elementType_ToBeDuplicated == null)
                    return null;

                elementType = elementType_ToBeDuplicated.Duplicate(construction_New.Name);
            }

            if (elementType == null)
                return null;

            Core.Revit.Modify.SetValues(elementType, construction_New);
            Core.Revit.Modify.SetValues(elementType, construction_New, ActiveSetting.Setting);

            return elementType;
        }

        public static ElementType DuplicateByType(this Document document, string name_Old, ApertureConstruction apertureConstruction_New)
        {
            if (apertureConstruction_New == null || document == null || string.IsNullOrWhiteSpace(name_Old))
                return null;

            BuiltInCategory builtInCategory = apertureConstruction_New.ApertureType.BuiltInCategory();
            if (builtInCategory == BuiltInCategory.INVALID)
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
                return null;

            ElementType elementType = elementTypes.Find(x => x.Name.Equals(apertureConstruction_New.Name));
            if (elementType == null)
            {
                ElementType elementType_ToBeDuplicated = elementTypes.Find(x => x.Name.Equals(name_Old));
                if (elementType_ToBeDuplicated == null)
                    return null;

                elementType = elementType_ToBeDuplicated.Duplicate(apertureConstruction_New.Name);
            }

            if (elementType == null)
                return null;

            Core.Revit.Modify.SetValues(elementType, apertureConstruction_New);
            Core.Revit.Modify.SetValues(elementType, apertureConstruction_New, ActiveSetting.Setting);

            return elementType;
        }
    }
}