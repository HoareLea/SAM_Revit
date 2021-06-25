using Autodesk.Revit.DB;
using SAM.Core;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static RevitInstance ToSAM(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null || !familyInstance.IsValidObject)
            {
                return null;
            }

            RevitInstance result = convertSettings?.GetObject<RevitInstance>(familyInstance.Id);
            if (result != null)
            {
                return result;
            }

            Document document = familyInstance.Document;
            if(document == null)
            {
                return result;
            }

            ElementType elementType = document.GetElement(familyInstance.GetTypeId()) as ElementType;
            if(elementType == null)
            {
                return null;
            }

            RevitType revitType = elementType.ToSAM(convertSettings);
            if(revitType == null)
            {
                return null;
            }

            if(revitType is RevitType3D)
            {
                result = new RevitInstance3D((RevitType3D)revitType, familyInstance.ToSAM_Geometries<Spatial.ISAMGeometry3D>());
            }
            else if(revitType is RevitType2D)
            {
                result = new RevitInstance2D((RevitType2D)revitType, familyInstance.ToSAM_Geometries<Planar.ISAMGeometry2D>());
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