using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<View> DuplicateViews(this Document document, string templateName_Source, IEnumerable<string> templateNames_Destionation, IEnumerable<Autodesk.Revit.DB.ViewType> viewTypes = null)
        {
            if (document == null || string.IsNullOrWhiteSpace(templateName_Source) || templateNames_Destionation == null || templateNames_Destionation.Count() == 0)
                return null;

            IEnumerable<View> views_All= new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>();
            if (views_All == null || views_All.Count() == 0)
                return null;

            List<View> views_Templates = new List<View>();
            List<View> views = new List<View>();
            foreach (View view in views_All)
            {
                if (viewTypes != null && viewTypes.Count() != 0 && !viewTypes.Contains(view.ViewType))
                    continue;

                if (view.IsTemplate)
                    views_Templates.Add(view);
                else
                    views.Add(view);
            }

            View view_Template_Source = views_Templates.Find(x => x.Name == templateName_Source);
            if (view_Template_Source == null)
                return null;

            List<View> views_Source = views.FindAll(x => x.ViewTemplateId == view_Template_Source.Id);
            if (views_Source == null || views_Source.Count == 0)
                return null;

            List<View> views_Template_Destination = templateNames_Destionation.ToList().ConvertAll(x => views_Templates.Find(y => y.Name == x));
            if (views_Template_Destination == null)
                return null;

            views_Template_Destination.RemoveAll(x => x == null);
            if (views_Template_Destination.Count == 0)
                return null;

            List<View> result = new List<View>();
            foreach (View view_Source in views_Source)
            {
                ElementId elementId_Level = view_Source.GenLevel.Id;
                ElementId elementId_ScopeBox = view_Source.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP)?.AsElementId();

                List<ElementId> elementIds_DependentView_ScopeBox = null;

                IEnumerable<ElementId> elementIds_DependentView = view_Source.GetDependentViewIds();
                if(elementIds_DependentView != null)
                {
                    elementIds_DependentView_ScopeBox = new List<ElementId>();
                    foreach(ElementId elementId in elementIds_DependentView)
                        elementIds_DependentView_ScopeBox.Add(document.GetElement(elementId)?.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP)?.AsElementId());
                }

                foreach(View view_Template_Destination in views_Template_Destination)
                {
                    ElementId elementId_View_New = view_Source.Duplicate(ViewDuplicateOption.WithDetailing);
                    if (elementId_View_New == null || elementId_View_New == ElementId.InvalidElementId)
                        continue;

                    View view_New = document.GetElement(elementId_View_New) as View;
                    if (view_New == null)
                        continue;

                    view_New.ViewTemplateId = view_Template_Destination.Id;
                    view_New.ApplyViewTemplateParameters(view_Source);
                    view_New.Name = string.Format("{0}_{1}", view_Source.GenLevel.Name, view_Template_Destination.Name);
                    view_New.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP)?.Set(elementId_ScopeBox);

                    result.Add(view_New);

                    if (elementIds_DependentView_ScopeBox != null && elementIds_DependentView_ScopeBox.Count() != 0)
                    {
                        foreach(ElementId elementId_DependentView_ScopeBox in elementIds_DependentView_ScopeBox)
                        {
                            if (elementId_DependentView_ScopeBox == null || elementId_DependentView_ScopeBox == ElementId.InvalidElementId)
                                continue;

                            ElementId elementId_View_New_Dependent = view_Source.Duplicate(ViewDuplicateOption.AsDependent);
                            if (elementId_View_New_Dependent == null || elementId_View_New_Dependent == ElementId.InvalidElementId)
                                continue;

                            View view_New_Dependent = document.GetElement(elementId_View_New_Dependent) as View;
                            if (view_New_Dependent  == null)
                                continue;

                            view_New_Dependent.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP)?.Set(elementId_DependentView_ScopeBox);

                            result.Add(view_New_Dependent);
                        }
                    }
                }
            }

            return result;

        }
    }
}