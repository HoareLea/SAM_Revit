using Autodesk.Revit.DB;
using System.Collections.Generic;
namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static ElementType DuplicateByName(this Document document, Construction construction_Old, PanelType panelType, string name_New, IEnumerable<string> parameterNames = null)
        {
            if (construction_Old == null || document == null || string.IsNullOrWhiteSpace(name_New))
            {
                return null;
            }

            ElementType result = Core.Revit.Modify.DuplicateByName(document, construction_Old, panelType.BuiltInCategory(), name_New, parameterNames);
            if (result == null)
            {
                return null;
            }

            Core.Revit.Modify.SetValues(result, construction_Old, ActiveSetting.Setting, null, parameterNames);

            return result;
        }

        public static ElementType DuplicateByName(this Document document, string name_Old, PanelType panelType, Construction construction_New, IEnumerable<string> parameterNames = null)
        {
            if (construction_New == null || document == null || string.IsNullOrWhiteSpace(name_Old))
            {
                return null;
            }

            ElementType result = Core.Revit.Modify.DuplicateByName(document, name_Old, panelType.BuiltInCategory(),construction_New, parameterNames);
            if(result == null)
            {
                return null;
            }

            Core.Revit.Modify.SetValues(result, construction_New, ActiveSetting.Setting, null, parameterNames);

            return result;
        }

        public static ElementType DuplicateByName(this Document document, ApertureConstruction apertureConstruction_Old, string name_New, IEnumerable<string> parameterNames = null)
        {
            if (apertureConstruction_Old == null || document == null || string.IsNullOrWhiteSpace(name_New))
            {
                return null;
            }

            ElementType result = Core.Revit.Modify.DuplicateByName(document, apertureConstruction_Old, apertureConstruction_Old.BuiltInCategory(), name_New, parameterNames);
            if (result == null)
            {
                return null;
            }

            Core.Revit.Modify.SetValues(result, apertureConstruction_Old, ActiveSetting.Setting, null, parameterNames);

            return result;
        }

        public static ElementType DuplicateByName(this Document document, string name_Old, ApertureConstruction apertureConstruction_New, IEnumerable<string> parameterNames = null)
        {
            if (apertureConstruction_New == null || document == null || string.IsNullOrWhiteSpace(name_Old))
            {
                return null;
            }

            ElementType result = Core.Revit.Modify.DuplicateByName(document, name_Old, apertureConstruction_New.BuiltInCategory(), apertureConstruction_New, parameterNames);
            if (result == null)
            {
                return null;
            }

            Core.Revit.Modify.SetValues(result, apertureConstruction_New, ActiveSetting.Setting, null, parameterNames);

            return result;
        }
    }
}