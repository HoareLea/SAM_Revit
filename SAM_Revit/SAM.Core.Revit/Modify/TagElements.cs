// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<IndependentTag> TagElements(this View view, ElementId elementId_TagType, BuiltInCategory builtInCategory, bool addLeader = false, TagOrientation tagOrientation = TagOrientation.Horizontal, bool allowDuplicates = false)
        {
            if (view == null || elementId_TagType == null || elementId_TagType == ElementId.InvalidElementId)
                return null;

            Document document = view.Document;

            FamilySymbol familySymbol = document.GetElement(elementId_TagType) as FamilySymbol;
            if (familySymbol == null)
                return null;

            BuiltInCategory builtInCategory_Tag = (BuiltInCategory)familySymbol.Category.Id.Value;


            if (!builtInCategory_Tag.IsValidTagCategory(builtInCategory))
                return null;

            IEnumerable<ElementId> elementIds = new FilteredElementCollector(document, view.Id).OfCategory(builtInCategory).ToElementIds();
            if (elementIds == null)
                return null;

            return TagElements(view, elementId_TagType, elementIds, addLeader, tagOrientation, allowDuplicates);
        }

        public static List<IndependentTag> TagElements(this View view, ElementId elementId_TagType, IEnumerable<ElementId> elementIds, bool addLeader = false, TagOrientation tagOrientation = TagOrientation.Horizontal, bool allowDuplicates = false)
        {
            if (view == null || elementId_TagType == null || elementId_TagType == ElementId.InvalidElementId)
                return null;

            Document document = view.Document;

            FamilySymbol familySymbol = document.GetElement(elementId_TagType) as FamilySymbol;
            if (familySymbol == null)
                return null;

            BuiltInCategory builtInCategory_Tag = (BuiltInCategory)familySymbol.Category.Id.Value;


            List<IndependentTag> result = new List<IndependentTag>();
            if (elementIds == null)
                return result;

            HashSet<ElementId> elementIds_Input = new HashSet<ElementId>(elementIds);
            elementIds_Input.Remove(null);
            elementIds_Input.Remove(ElementId.InvalidElementId);
            if (elementIds_Input.Count == 0)
                return result;

            ICollection<ElementId> elementIds_View = new FilteredElementCollector(document, view.Id).WhereElementIsNotElementType().WherePasses(new ElementIdSetFilter(elementIds_Input.ToList())).ToElementIds();
            if (elementIds_View == null || elementIds_View.Count == 0)
                return result;

            HashSet<ElementId> elementIds_Tagged = null;
            if (!allowDuplicates)
            {
                elementIds_Tagged = new HashSet<ElementId>();
                // Owner-view filtered, not view-scoped: a tag hidden in the view must still suppress a
                // duplicate. ElementOwnerViewFilter gives that same semantics natively, so Revit does the
                // narrowing instead of this loop walking every tag in the document once per view.
                foreach (IndependentTag independentTag_Existing in new FilteredElementCollector(document).OfCategory(builtInCategory_Tag).OfClass(typeof(IndependentTag)).WherePasses(new ElementOwnerViewFilter(view.Id)).Cast<IndependentTag>())
                {
                    if (independentTag_Existing.GetTypeId() != elementId_TagType)
                        continue;

                    ICollection<ElementId> elementIds_Tagged_Temp = independentTag_Existing.GetTaggedLocalElementIds();
                    if (elementIds_Tagged_Temp == null)
                        continue;

                    foreach (ElementId elementId_Tagged in elementIds_Tagged_Temp)
                        elementIds_Tagged.Add(elementId_Tagged);
                }
            }

            foreach (ElementId elementId in elementIds_View)
            {
                Element element = document.GetElement(elementId);
                if (element == null)
                    continue;

                if (!allowDuplicates && elementIds_Tagged.Contains(elementId))
                    continue;

                if (!builtInCategory_Tag.IsValidTagCategory((BuiltInCategory)element.Category.Id.Value))
                    continue;



                Autodesk.Revit.DB.Location location = element.Location;
                if (location == null)
                    continue;

                XYZ xyz = null;
                if (location is LocationCurve)
                {
                    LocationCurve locationCurve = (LocationCurve)location;
                    Curve curve = locationCurve.Curve;
                    if (curve == null)
                        continue;

                    XYZ xyz_1 = curve.GetEndPoint(0);
                    XYZ xyz_2 = curve.GetEndPoint(1);
                    xyz = new XYZ((xyz_1.X + xyz_2.X) / 2, (xyz_1.Y + xyz_2.Y) / 2, (xyz_1.Z + xyz_2.Z) / 2);
                }
                else if (location is LocationPoint)
                {
                    xyz = ((LocationPoint)location).Point;
                }

                if (xyz == null)
                    continue;

                Autodesk.Revit.DB.Reference reference = new Autodesk.Revit.DB.Reference(element);
                IndependentTag independentTag = IndependentTag.Create(document, elementId_TagType, view.Id, reference, addLeader, tagOrientation, xyz);

                if (independentTag != null)
                    result.Add(independentTag);
            }

            return result;
        }
    
        public static List<IndependentTag> TagElements(this Document document, IEnumerable<string> templateNames, ElementId elementId_TagType, IEnumerable<ElementId> elementIds, bool addLeader = false, TagOrientation tagOrientation = TagOrientation.Horizontal, IEnumerable<Autodesk.Revit.DB.ViewType> viewTypes = null, bool allowDuplicates = false)
        {
            if (templateNames == null || elementId_TagType == null || elementId_TagType == ElementId.InvalidElementId)
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

            List<IndependentTag> result = new List<IndependentTag>();

            foreach(string name in templateNames)
            {
                View view_Template = views_Templates.Find(x => x.Name == name);
                if (view_Template == null)
                    continue;

                List<View> views_Temp = views.FindAll(x => x.ViewTemplateId == view_Template.Id);
                if (views_Temp == null || views_Temp.Count == 0)
                    continue;

                List<ElementId> elementIds_DependentView = new List<ElementId>();
                foreach(View view_Temp in views_Temp)
                {
                    IEnumerable<ElementId> elementIds_DependentView_Temp = view_Temp.GetDependentViewIds();
                    if (elementIds_DependentView_Temp == null || elementIds_DependentView_Temp.Count() == 0)
                        continue;

                    elementIds_DependentView.AddRange(elementIds_DependentView_Temp);
                }

                foreach(View view in views_Temp)
                {
                    if (elementIds_DependentView.Contains(view.Id))
                        continue;

                    List<IndependentTag> independentTags = TagElements(view, elementId_TagType, elementIds, addLeader, tagOrientation, allowDuplicates);
                    if (independentTags != null && independentTags.Count != 0)
                        result.AddRange(independentTags);
                }
            }

            return result;
        }
    }
}