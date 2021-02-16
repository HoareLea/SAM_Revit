using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<ElementId> DeleteViews(this Document document, string templateName, bool inverted = false, IEnumerable<Autodesk.Revit.DB.ViewType> viewTypes = null)
        {
            if (document == null || string.IsNullOrEmpty(templateName))
                return null;

            IEnumerable<View> views_All = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>();

            List<View> views_Template = new List<View>();
            List<View> views = new List<View>();

            foreach(View view in views_All)
            {
                if (viewTypes != null && viewTypes.Count() != 0 && !viewTypes.Contains(view.ViewType))
                    continue;

                if (view.IsTemplate)
                    views_Template.Add(view);
                else
                    views.Add(view);
            }

            View view_Template = views_Template.Find(x => x.Name == templateName);
            if (view_Template == null)
                return null;

            List<ElementId> result = new List<ElementId>();
            foreach(View view in views)
            {
                bool remove = view.ViewTemplateId == view_Template.Id;
                if (inverted)
                    remove = !remove;

                if (remove)
                    result.Add(view.Id);
            }

            if (result != null && result.Count != 0)
                document.Delete(result);

            return result;
        }
    }
}