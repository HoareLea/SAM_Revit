using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Create
    {
        public static CategorySet CategorySet(Document document, IEnumerable<BuiltInCategory> builtInCategories)
        {
            if(document == null || builtInCategories == null)
            {
                return null;
            }

            CategorySet result = new CategorySet();
            foreach(BuiltInCategory builtInCategory in builtInCategories)
            {
                Category category = document.Settings.Categories.get_Item(builtInCategory);
                if(category == null)
                {
                    continue;
                }

                result.Insert(category);
            }

            return result;
        }
    }
}