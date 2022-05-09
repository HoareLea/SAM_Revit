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

            if (!sAMObject.TryGetValue(ElementParameter.RevitId, out IntegerId integerId) || integerId == null)
            {
                return null;
            }

            if(!integerId.TryGetValue(RevitIdParameter.UniqueId, out string result))
            {
                return null;
            }

            return result;
        }
    }
}