using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string UniqueId(this SAMObject sAMObject)
        {
            if (sAMObject == null)
                return null;

            string uniqueId;
            if (!sAMObject.TryGetValue(ElementParameter.UniqueId, out uniqueId))
                return null;

            return uniqueId;
        }
    }
}