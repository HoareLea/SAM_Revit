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

            ParameterSet parameterSet = new ParameterSet(element.GetType()?.Assembly);
            parameterSet.Add("Name", Query.FullName(element));

            string categoryName = null;
            if (element is Family)
                categoryName = ((Family)element).FamilyCategory?.Name;
            else
                categoryName = element.Category?.Name;

            parameterSet.Add("Category Name", categoryName);

            integerId.Add(parameterSet);
            return integerId;
        }
    }
}