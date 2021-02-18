using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<SpaceTag> TagSpaces(this View view, ElementId elementId_Tag)
        {
            if (view == null || elementId_Tag == null || elementId_Tag == ElementId.InvalidElementId)
                return null;

            Document document = view.Document;

            SpaceTagType spaceTagType = document.GetElement(elementId_Tag) as SpaceTagType;
            if (spaceTagType == null)
                return null;

            IEnumerable<Space> spaces = new FilteredElementCollector(document, view.Id).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Space>();
            if (spaces == null)
                return null;

            List<SpaceTag> result = new List<SpaceTag>();
            if (spaces.Count() == 0)
                return result;

            foreach (Space space in spaces)
            {
                if (space == null || !space.IsValidObject)
                    continue;

                Autodesk.Revit.DB.Location location = space.Location;
                if (location == null)
                    continue;

                XYZ xyz = null;
                if (location is LocationPoint)
                    xyz = ((LocationPoint)location).Point;

                if (xyz == null)
                    continue;

                SpaceTag spaceTag = document.Create.NewSpaceTag(space, new UV(xyz.X, xyz.Y), view);
                if (spaceTag == null)
                    continue;

                if (spaceTag.GetTypeId() != elementId_Tag)
                    spaceTag.SpaceTagType = spaceTagType;

                result.Add(spaceTag);
            }

            return result;
        }

        public static List<SpaceTag> TagSpaces(this Document document, IEnumerable<string> templateNames, ElementId elementId_Tag, IEnumerable<Autodesk.Revit.DB.ViewType> viewTypes = null)
        {
            if (document == null || elementId_Tag == null || elementId_Tag == ElementId.InvalidElementId || templateNames == null || templateNames.Count() == 0)
                return null;

            List<View> views_All = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();

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

            List<SpaceTag> result = new List<SpaceTag>();
            foreach (string templateName in templateNames)
            {
                View view_Template = views_Templates.Find(x => x.Name == templateName);
                if (view_Template == null)
                    continue;

                List<View> views_Temp = views.FindAll(x => x.ViewTemplateId == view_Template.Id);
                if (views_Temp == null || views_Temp.Count == 0)
                    continue;

                List<ElementId> elementIds_DependentView = new List<ElementId>();
                foreach (View view_Temp in views_Temp)
                {
                    IEnumerable<ElementId> elementIds_DependentView_Temp = view_Temp.GetDependentViewIds();
                    if (elementIds_DependentView_Temp == null || elementIds_DependentView_Temp.Count() == 0)
                        continue;

                    elementIds_DependentView.AddRange(elementIds_DependentView_Temp);
                }

                foreach (View view in views_Temp)
                {
                    if (elementIds_DependentView.Contains(view.Id))
                        continue;

                    List<SpaceTag> spaceTags = TagSpaces(view, elementId_Tag);
                    if (spaceTags != null)
                        result.AddRange(spaceTags);
                }
            }

            return result;
        }
    }
}