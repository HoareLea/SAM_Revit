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

            ElementId elementId_Level = spatialElement.LevelId;
            if(elementId_Level != null && elementId_Level != ElementId.InvalidElementId)
            {
                Level level = spatialElement.Document?.GetElement(elementId_Level) as Level;
                if (level != null)
                    result.SetValue(SpaceParameter.LevelName, level.Name);
            }

            double area;
            if (!result.TryGetValue(SpaceParameter.Area, out area) || double.IsNaN(area) || area == 0)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
            result.SetValue(SpaceParameter.Area, UnitUtils.ConvertFromInternalUnits(spatialElement.Area, DisplayUnitType.DUT_SQUARE_METERS));
#else
                result.SetValue(SpaceParameter.Area, UnitUtils.ConvertFromInternalUnits(spatialElement.Area, UnitTypeId.SquareMeters));
#endif
            }

            double volume;
            if (!result.TryGetValue(SpaceParameter.Volume, out volume) || double.IsNaN(volume) || volume == 0)
            {
                Parameter parameter = spatialElement.get_Parameter(BuiltInParameter.ROOM_VOLUME);
                if (parameter != null && parameter.HasValue)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    result.SetValue(SpaceParameter.Volume, UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), DisplayUnitType.DUT_CUBIC_METERS));
#else
                    result.SetValue(SpaceParameter.Volume, UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), UnitTypeId.CubicMeters));
#endif
                }

            }

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