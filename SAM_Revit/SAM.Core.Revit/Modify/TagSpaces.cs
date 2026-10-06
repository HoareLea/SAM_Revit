// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<SpaceTag> TagSpaces(this View view, ElementId elementId_SpaceTagType, bool allowDuplicates = false)
        {
            if (view == null || elementId_SpaceTagType == null || elementId_SpaceTagType == ElementId.InvalidElementId)
                return null;

            Document document = view.Document;

            SpaceTagType spaceTagType = document.GetElement(elementId_SpaceTagType) as SpaceTagType;
            if (spaceTagType == null)
                return null;

            List<Space> spaces = new FilteredElementCollector(document, view.Id).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Space>().ToList();
            if (spaces == null)
                return null;

            List<SpaceTag> result = new List<SpaceTag>();
            if (spaces.Count == 0)
                return result;

            // Precomputed once per view, rather than a GetDependentElements call plus a GetElement per
            // dependent id for every space. ElementOwnerViewFilter keeps the same owner-based semantics
            // the LogicalAndFilter had, so a tag hidden in the view still suppresses a duplicate.
            HashSet<ElementId> elementIds_Tagged = null;
            if (!allowDuplicates)
            {
                elementIds_Tagged = new HashSet<ElementId>();
                foreach (SpaceTag spaceTag_Existing in new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaceTags).WhereElementIsNotElementType().WherePasses(new ElementOwnerViewFilter(view.Id)).Cast<SpaceTag>())
                {
                    if (spaceTag_Existing.GetTypeId() != elementId_SpaceTagType)
                        continue;

                    Space space_Tagged = spaceTag_Existing.Space;
                    if (space_Tagged != null)
                        elementIds_Tagged.Add(space_Tagged.Id);
                }
            }

            foreach (Space space in spaces)
            {
                if (space == null || !space.IsValidObject)
                    continue;

                if (!allowDuplicates && elementIds_Tagged.Contains(space.Id))
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

                if (spaceTag.GetTypeId() != elementId_SpaceTagType)
                    spaceTag.SpaceTagType = spaceTagType;

                result.Add(spaceTag);
            }

            return result;
        }

        public static List<SpaceTag> TagSpaces(this Document document, IEnumerable<string> templateNames, ElementId elementId_Tag, IEnumerable<Autodesk.Revit.DB.ViewType> viewTypes = null, bool allowDuplicates = false)
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

                    List<SpaceTag> spaceTags = TagSpaces(view, elementId_Tag, allowDuplicates);
                    if (spaceTags != null)
                        result.AddRange(spaceTags);
                }
            }

            return result;
        }
    }
}