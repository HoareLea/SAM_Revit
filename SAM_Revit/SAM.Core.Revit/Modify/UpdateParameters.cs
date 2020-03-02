using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using System.Reflection;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool UpdateParameters(this ParameterSet parameterSet, Element element, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (parameterSet == null || element == null)
                return false;

            foreach(string name in parameterSet.Names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (parameterNames_Excluded != null && parameterNames_Excluded.Contains(name))
                    continue;

                UpdateParameter(element.LookupParameter(name), parameterSet.ToObject(name));
            }

            return true;
        }

        public static bool UpdateParameters(this SAMObject sAMObject, Element element, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (sAMObject == null || element == null)
                return false;

            return UpdateParameters(sAMObject.GetParameterSet(element.GetType()?.Assembly), element, parameterNames_Excluded);
        }
    }
}
