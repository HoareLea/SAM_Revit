namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string UniqueId(this SAMObject sAMObject)
        {
            if (sAMObject == null)
            {
                return null;
            }

            if (!sAMObject.TryGetValue(ElementParameter.RevitId, out LongId longId) || longId == null)
            {
                return null;
            }

            return UniqueId(longId);
        }

        public static string UniqueId(this LongId longId)
        {
            if (longId == null)
            {
                return null;
            }

            if (!longId.TryGetValue(RevitIdParameter.UniqueId, out string result))
            {
                return null;
            }

            return result;
        }
    }
}