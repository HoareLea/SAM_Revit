namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string ParameterNamePrefix(IResult result)
        {
            if (result == null)
                return null;

            string source = result.Source;
            if(!string.IsNullOrWhiteSpace(source))
            {
                if (source.ToUpper().EndsWith("TAS"))
                    return "_T";
                else if (source.ToUpper().EndsWith("OPENSTUDIO"))
                    return "_E";
            }


            return string.Empty;
        }
    }
}