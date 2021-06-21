using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static OpeningType ToSAM_OpeningType(this FamilySymbol familySymbol, ConvertSettings convertSettings)
        {
            if (familySymbol == null)
                return null;

            OpeningType result = convertSettings?.GetObject<OpeningType>(familySymbol.Id);
            if (result != null)
                return result;

            string name = familySymbol.Name;

            switch ((BuiltInCategory)familySymbol.Category.Id.IntegerValue)
            {
                case BuiltInCategory.OST_Windows:
                case BuiltInCategory.OST_CurtainWallPanels:
                    result = new WindowType(name);
                    break;

                case BuiltInCategory.OST_Doors:
                    result = new DoorType(name);
                    break;
            }

            result.UpdateParameterSets(familySymbol, ActiveSetting.Setting.GetValue<Core.TypeMap>(ActiveSetting.Name.ParameterMap));

            convertSettings?.Add(familySymbol.Id, result);

            return result;
        }
    }
}