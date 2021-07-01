namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static DesignOption ToSAM(this Autodesk.Revit.DB.DesignOption designOption, ConvertSettings convertSettings)
        {
            if (designOption == null)
            {
                return null;
            }

            DesignOption result = convertSettings?.GetObject<DesignOption>(designOption.Id);
            if (result != null)
            {
                return result;
            }

            result = new DesignOption(designOption.Name, designOption.IsPrimary);

            result.UpdateParameterSets(designOption, ActiveSetting.Setting.GetValue<TypeMap>(ActiveSetting.Name.ParameterMap));

            if (result != null)
            {
                convertSettings?.Add(designOption.Id, result);
            }

            return result;
        }
    }
}