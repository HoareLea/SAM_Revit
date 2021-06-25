using Autodesk.Revit.DB;
using SAM.Core;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static RevitInstance3D ToSAM(this WallSweep wallSweep, ConvertSettings convertSettings)
        {
            if (wallSweep == null || !wallSweep.IsValidObject)
            {
                return null;
            }

            RevitInstance3D result = convertSettings?.GetObject<RevitInstance3D>(wallSweep.Id);
            if (result != null)
            {
                return result;
            }

            Document document = wallSweep.Document;
            if(document == null)
            {
                return result;
            }

            ElementType elementType = document.GetElement(wallSweep.GetTypeId()) as ElementType;
            if(elementType == null)
            {
                return null;
            }

            RevitType3D revitType3D = elementType.ToSAM(convertSettings) as RevitType3D;
            if(revitType3D == null)
            {
                return null;
            }

            result = new RevitInstance3D(revitType3D, wallSweep.ToSAM_Geometries<Spatial.ISAMGeometry3D>());

            if (result != null)
            {
                result.UpdateParameterSets(elementType, ActiveSetting.Setting.GetValue<TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
                convertSettings?.Add(elementType.Id, result);
            }

            return result;
        }
    }
}