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

            Autodesk.Revit.DB.Category category = element is Family ? ((Family)element).FamilyCategory : element.Category;
            if(category != null)
            {
                integerId.SetValue(RevitIdParameter.CategoryName, category.Name);
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                integerId.SetValue(RevitIdParameter.CategoryId, category.Id.IntegerValue);
#else
                integerId.SetValue(RevitIdParameter.CategoryId, category.Id.Value);
#endif

            }

            integerId.SetValue(RevitIdParameter.UniqueId, element.UniqueId);

            return integerId;
        }
    }
}