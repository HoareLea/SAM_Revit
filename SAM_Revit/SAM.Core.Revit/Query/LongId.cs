using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static LongId LongId(this Element element)
        {
            if (element == null)
                return null;

            LongId result = Convert.ToSAM(element.Id);
            if (result == null)
                return null;

            string fullName = FullName(element);
            if(!string.IsNullOrEmpty(fullName))
            {
                result.SetValue(RevitIdParameter.FullName, fullName);
            }

            Autodesk.Revit.DB.Category category = element is Family ? ((Family)element).FamilyCategory : element.Category;
            if(category != null)
            {
                result.SetValue(RevitIdParameter.CategoryName, category.Name);
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                result.SetValue(RevitIdParameter.CategoryId, category.Id.IntegerValue);
#else
                result.SetValue(RevitIdParameter.CategoryId, category.Id.Value);
#endif
            }

            result.SetValue(RevitIdParameter.UniqueId, element.UniqueId);

            return result;
        }
    }
}