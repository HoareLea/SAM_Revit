using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SAM.Core.Revit.Addin
{
    public class AlwaysAvailableExternalCommandAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }
    }
}
