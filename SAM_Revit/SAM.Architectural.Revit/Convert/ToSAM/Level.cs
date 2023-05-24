using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static Level ToSAM(this Autodesk.Revit.DB.Level level, ConvertSettings convertSettings)
        {
            if (level == null)
                return null;

            Document document = level.Document;

            Level result = convertSettings?.GetObject<Level>(level.Id);
            if (result != null)
                return result;

            double elevation = Query.Elevation(level);

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(level.Document);
                if (transform != null)
                {
                    elevation = Geometry.Revit.Query.Transform(transform, new Geometry.Spatial.Point3D(0,0, elevation), false).Z;
                }
            }

            result = new Level(level.Name, elevation);
            result.UpdateParameterSets(level, ActiveSetting.Setting.GetValue<Core.TypeMap>(ActiveSetting.Name.ParameterMap));
            //result.Add(Core.Revit.Query.ParameterSet(level));

            convertSettings?.Add(level.Id, result);

            return result;
        }
    }
}