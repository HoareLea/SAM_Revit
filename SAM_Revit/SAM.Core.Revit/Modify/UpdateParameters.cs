using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool UpdateParameters(this ParameterSet parameterSet, Element element)
        {
            if (parameterSet == null || element == null)
                return false;

            foreach(string name in parameterSet.Names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                UpdateParameter(element.LookupParameter(name), parameterSet.ToObject(name));
            }

            return true;
        }
    }
}
