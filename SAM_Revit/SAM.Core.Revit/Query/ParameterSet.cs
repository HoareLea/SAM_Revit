namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ParameterSet ParameterSet(this Autodesk.Revit.DB.Element element)
        {
            ParameterSet parameterSet = new ParameterSet("Revit");
            parameterSet.Add("ElementId", element.Id.IntegerValue);
            return parameterSet;
        }
    }
}
