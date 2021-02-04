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
            }

            return string.Empty;
        }
    }
}