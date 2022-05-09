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

            Modify.SetValues(spaceTagType, result);

            if (result != null)
            {
                convertSettings?.Add(spaceTagType.Id, result);
            }

            return result;
        }
    }
}