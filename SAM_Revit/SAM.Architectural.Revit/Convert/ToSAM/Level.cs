using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static Level ToSAM(this Autodesk.Revit.DB.Level level, Core.Revit.ConvertSettings convertSettings)
        {
            if (level == null)
                return null;

            Document document = level.Document;

            Level result = convertSettings?.GetObject<Level>(level.Id);
            if (result != null)
                return result;

            double elevation = Query.Elevation(level);

            result = new Level(level.Name, elevation);
            result.UpdateParameterSets(level, ActiveSetting.Setting.GetValue<Core.TypeMap>(ActiveSetting.Name.ParameterMap));
            //result.Add(Core.Revit.Query.ParameterSet(level));

            convertSettings?.Add(level.Id, result);

            return result;
        }
    }
}