using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static TagType ToSAM(this SpaceTagType spaceTagType, ConvertSettings convertSettings)
        {
            if (spaceTagType == null)
                return null;

            TagType result = convertSettings?.GetObject<TagType>(spaceTagType.Id);
            if (result != null)
                return result;

            result = new TagType(spaceTagType.Name);
            if (result != null)
            {
                Modify.SetValues(spaceTagType, result);
                result.SetValue(ElementParameter.RevitId, Query.LongId(spaceTagType));
                convertSettings?.Add(spaceTagType.Id, result);
            }

            return result;
        }

        public static TagType ToSAM_TagType(this FamilySymbol familySymbol, ConvertSettings convertSettings)
        {
            if (familySymbol == null)
                return null;

            TagType result = convertSettings?.GetObject<TagType>(familySymbol.Id);
            if (result != null)
                return result;

            result = new TagType(familySymbol.Name);
            if (result != null)
            {
                Modify.SetValues(familySymbol, result);
                result.SetValue(ElementParameter.RevitId, Query.LongId(familySymbol));
                convertSettings?.Add(familySymbol.Id, result);
            }

            return result;
        }
    }
}