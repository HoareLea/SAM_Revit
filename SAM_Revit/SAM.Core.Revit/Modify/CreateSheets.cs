using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<ViewSheet> CreateSheets(this ViewSheet referenceViewSheet, IEnumerable<string> templateNames)
        {
            if (referenceViewSheet == null || templateNames == null || templateNames.Count() == 0)
                return null;

            Document document = referenceViewSheet.Document;

            FamilyInstance familyInstance_TitleBlock = new FilteredElementCollector(document, referenceViewSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().FirstOrDefault();
            if (familyInstance_TitleBlock == null)
                return null;

            Viewport viewport = new FilteredElementCollector(document, referenceViewSheet.Id).OfClass(typeof(Viewport)).Cast<Viewport>().FirstOrDefault();
            if (viewport == null)
                return null;

            View view_Source = document.GetElement(viewport.ViewId) as View;
            if (view_Source == null)
                return null;

            XYZ xyz = viewport.GetBoxCenter();
            if (xyz == null)
                return null;

            ElementId elementId_TitleBlockType = familyInstance_TitleBlock.GetTypeId();
            if (elementId_TitleBlockType == null || elementId_TitleBlockType == ElementId.InvalidElementId)
                return null;

            IEnumerable<View> views_All = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>();
            if (views_All == null || views_All.Count() == 0)
                return null;

            List<View> views_Templates = new List<View>();
            List<View> views = new List<View>();
            foreach (View view in views_All)
            {
                if (view is ViewSheet)
                    continue;

                if (view_Source.ViewType != view.ViewType || view_Source.Id == view.Id)
                    continue;

                if (view.IsTemplate)
                    views_Templates.Add(view);
                else
                    views.Add(view);
            }

            List<ElementId> elementIds = new List<ElementId>();
            foreach(View view_Template in views_Templates)
            {
                if (!templateNames.Contains(view_Template.Name))
                    continue;

                elementIds.Add(view_Template.Id);
            }

            if (elementIds == null || elementIds.Count == 0)
                return null;

            List<ViewSheet> result = new List<ViewSheet>();
            foreach(View view in views)
            {
                if (!elementIds.Contains(view.ViewTemplateId))
                    continue;

                View viewTemplate = document.GetElement(view.ViewTemplateId) as View;

                ViewSheet viewSheet_New = ViewSheet.Create(document, elementId_TitleBlockType);
                if (viewSheet_New == null)
                    continue;

                try
                {
                    viewSheet_New.Name = string.Format("{0}_{1}", viewTemplate.Name, view.GenLevel.Name);
                }
                catch(System.Exception exception)
                {

                }

                Viewport viewport_New = Viewport.Create(document, viewSheet_New.Id, view.Id, xyz);

                result.Add(viewSheet_New);
            }

            return result;
        }
    }
}