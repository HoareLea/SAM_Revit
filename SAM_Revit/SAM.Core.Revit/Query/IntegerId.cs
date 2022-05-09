using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static IntegerId IntegerId(this Element element)
        {
            if (element == null)
                return null;

            IntegerId integerId = Convert.ToSAM(element.Id);
            if (integerId == null)
                return null;

            string fullName = FullName(element);
            if(!string.IsNullOrEmpty(fullName))
            {
                integerId.SetValue(RevitIdParameter.FullName, fullName);
            }
            string categoryName = element is Family ? ((Family)element).FamilyCategory?.Name : element.Category?.Name;
            if(!string.IsNullOrEmpty(categoryName))
            {
                integerId.SetValue(RevitIdParameter.CategoryName, categoryName);
            }

            integerId.SetValue(RevitIdParameter.UniqueId, element.UniqueId);

            return integerId;
        }
    }
}