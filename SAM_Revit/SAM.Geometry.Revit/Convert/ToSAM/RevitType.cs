using Autodesk.Revit.DB;
using SAM.Core;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static RevitType ToSAM(this ElementType elementType, ConvertSettings convertSettings)
        {
            if (elementType == null)
            {
                return null;
            }

            RevitType result = convertSettings?.GetObject<RevitType>(elementType.Id);
            if (result != null)
            {
                return result;
            }

            switch(elementType.Category.CategoryType)
            {
                case CategoryType.AnalyticalModel:
                case CategoryType.Model:
                    result = new RevitType3D(elementType.Name);
                    break;

                case CategoryType.Annotation:
                    result = new RevitType2D(elementType.Name);
                    break;
            }

            if (result != null)
            {
                result.UpdateParameterSets(elementType, ActiveSetting.Setting.GetValue<TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
                convertSettings?.Add(elementType.Id, result);
            }

            return result;
        }
    }
}