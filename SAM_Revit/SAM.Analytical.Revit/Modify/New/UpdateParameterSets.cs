namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static void UpdateParameterSets(this IAnalyticalObject analyticalObject, Autodesk.Revit.DB.Element element)
        {
            Core.SAMObject sAMObject = analyticalObject as Core.SAMObject;
            if(sAMObject == null)
            {
                return;
            }

            Core.Revit.Modify.UpdateParameterSets(sAMObject, element, Core.Revit.ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
        }
    }
}