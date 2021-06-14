using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static ElementType DuplicateByName(this Document document, string name_Old, BuiltInCategory builtInCategory, SAMType sAMType_New, IEnumerable<string> parameterNames = null)
        {
            if (sAMType_New == null || document == null || string.IsNullOrWhiteSpace(name_Old) || builtInCategory == BuiltInCategory.INVALID)
                return null;

            string name_New = sAMType_New.Name;
            if (string.IsNullOrWhiteSpace(name_New))
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
                return null;

            ElementType elementType = elementTypes.Find(x => x.Name.Equals(name_New));
            if (elementType == null)
            {
                ElementType elementType_ToBeDuplicated = elementTypes.Find(x => x.Name.Equals(name_Old));
                if (elementType_ToBeDuplicated == null)
                    return null;

                elementType = elementType_ToBeDuplicated.Duplicate(name_New);
            }

            if (elementType == null)
                return null;

            SetValues(elementType, sAMType_New, parameterNames);

            return elementType;
        }

        public static ElementType DuplicateByName(this Document document, Core.SAMType samType_Old, BuiltInCategory builtInCategory, string name_New, IEnumerable<string> parameterNames = null)
        {
            if (samType_Old == null || document == null || string.IsNullOrWhiteSpace(name_New) || builtInCategory == BuiltInCategory.INVALID)
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
            if (elementTypes == null || elementTypes.Count == 0)
                return null;

            ElementType elementType = elementTypes.Find(x => x.Name.Equals(name_New));
            if (elementType == null)
            {
                ElementType elementType_ToBeDuplicated = elementTypes.Find(x => x.Name.Equals(samType_Old.Name));
                if (elementType_ToBeDuplicated == null)
                    return null;

                elementType = elementType_ToBeDuplicated.Duplicate(name_New);
            }

            if (elementType == null)
                return null;

            SetValues(elementType, samType_Old, parameterNames);

            return elementType;
        }
    }
}