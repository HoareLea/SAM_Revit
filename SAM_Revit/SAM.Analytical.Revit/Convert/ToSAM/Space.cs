using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Space ToSAM(this SpatialElement spatialElement, ConvertSettings convertSettings)
        {
            if (spatialElement == null)
                return null;
            
            Point3D point3D = null;

            Space result = convertSettings?.GetObject<Space>(spatialElement.Id);
            if (result != null)
                return result;

            if (spatialElement.Location != null)
                point3D = ((LocationPoint)spatialElement.Location).Point.ToSAM();

            string name = Core.Revit.Query.Name(spatialElement);
            if (string.IsNullOrWhiteSpace(name))
                name = spatialElement.Name;

            result = new Space(name, point3D);
            result.UpdateParameterSets(spatialElement, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

            result.InternalCondition = ToSAM_InternalCondition(spatialElement, convertSettings);

            convertSettings?.Add(spatialElement.Id, result);

            return result;
        }

        public static Space ToSAM(this EnergyAnalysisSpace energyAnalysisSpace, ConvertSettings convertSettings)
        {
            if (energyAnalysisSpace == null)
                return null;

            Document document = energyAnalysisSpace.Document;

            SpatialElement spatialElement = Core.Revit.Query.Element(document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (spatialElement == null)
                return null;

            return ToSAM(spatialElement, convertSettings);

        }
    }
}