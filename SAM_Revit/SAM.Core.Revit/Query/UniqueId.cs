using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string UniqueId(this SAMObject sAMObject)
        {
            if (sAMObject == null)
                return null;

            ParameterSet parameterSet = sAMObject.GetParameterSet(typeof(Element).Assembly);
            if (parameterSet == null)
                return null;

            return parameterSet.ToString("UniqueId");
        }
    }
}