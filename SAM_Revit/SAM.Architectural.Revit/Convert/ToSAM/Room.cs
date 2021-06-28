using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static Room ToSAM(this SpatialElement spatialElement, ConvertSettings convertSettings)
        {
            if (spatialElement == null)
                return null;
            
            Point3D point3D = null;

            Room result = convertSettings?.GetObject<Room>(spatialElement.Id);
            if (result != null)
                return result;

            if (spatialElement.Location != null)
                point3D = ((LocationPoint)spatialElement.Location).Point.ToSAM();

            string name = Core.Revit.Query.Name(spatialElement);
            if (string.IsNullOrWhiteSpace(name))
                name = spatialElement.Name;

            result = new Room(name, point3D);
            result.UpdateParameterSets(spatialElement, Core.ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

            ElementId elementId_Level = spatialElement.LevelId;
            if(elementId_Level != null && elementId_Level != ElementId.InvalidElementId)
            {
                Autodesk.Revit.DB.Level level = spatialElement.Document?.GetElement(elementId_Level) as Autodesk.Revit.DB.Level;
                if (level != null)
                    result.SetValue(RoomParameter.LevelName, level.Name);
            }

            double area;
            if (!result.TryGetValue(RoomParameter.Area, out area) || double.IsNaN(area) || area == 0)
                result.SetValue(RoomParameter.Area, UnitUtils.ConvertFromInternalUnits(spatialElement.Area, DisplayUnitType.DUT_SQUARE_METERS));

            double volume;
            if (!result.TryGetValue(RoomParameter.Volume, out volume) || double.IsNaN(volume) || volume == 0)
            {
                Parameter parameter = spatialElement.get_Parameter(BuiltInParameter.ROOM_VOLUME);
                if (parameter != null && parameter.HasValue)
                    result.SetValue(RoomParameter.Volume, UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), DisplayUnitType.DUT_CUBIC_METERS));
            }

            convertSettings?.Add(spatialElement.Id, result);

            return result;
        }

        public static Room ToSAM(this EnergyAnalysisSpace energyAnalysisSpace, ConvertSettings convertSettings)
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