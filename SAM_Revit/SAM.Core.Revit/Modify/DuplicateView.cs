using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<View> DuplicateView(this View view, IEnumerable<Level> levels)
        {
            if (view == null || !view.IsValidObject || view.IsTemplate)
                return null;

            Document document = view.Document;
            if (document == null)
                return null;

            throw new System.NotImplementedException();

            //List<Level> levels_Temp = 

        }
    }
}