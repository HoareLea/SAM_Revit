using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<IndependentTag> TagElements(this View view, ElementId elementId_Tag, BuiltInCategory builtInCategory, bool addLeader = false, TagOrientation tagOrientation = TagOrientation.Horizontal)
        {
            if (view == null || elementId_Tag == null || elementId_Tag == ElementId.InvalidElementId)
                return null;

            Document document = view.Document;

            FamilySymbol familySymbol = document.GetElement(elementId_Tag) as FamilySymbol;
            if (familySymbol == null)
                return null;

            BuiltInCategory builtInCategory_Tag = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            if (!builtInCategory_Tag.IsValidTagCategory(builtInCategory))
                return null;

            IEnumerable<ElementId> elementIds = new FilteredElementCollector(document, view.Id).OfCategory(builtInCategory).ToElementIds();
            if (elementIds == null)
                return null;

            return TagElements(view, elementId_Tag, elementIds, addLeader, tagOrientation);
        }

        public static List<IndependentTag> TagElements(this View view, ElementId elementId_Tag, IEnumerable<ElementId> elementIds, bool addLeader = false, TagOrientation tagOrientation = TagOrientation.Horizontal)
        {
            if (view == null || elementId_Tag == null || elementId_Tag == ElementId.InvalidElementId)
                return null;

            Document document = view.Document;

            FamilySymbol familySymbol = document.GetElement(elementId_Tag) as FamilySymbol;
            if (familySymbol == null)
                return null;

            BuiltInCategory builtInCategory_Tag = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;

            IEnumerable<ElementId> elementIds_View = new FilteredElementCollector(document, view.Id).ToElementIds();
            if (elementIds_View == null)
                return null;

            List<IndependentTag> result = new List<IndependentTag>();
            if (elementIds_View.Count() == 0)
                return result;

            foreach (ElementId elementId in elementIds_View)
            {
                if (elementId == null || elementId != ElementId.InvalidElementId)
                    continue;

                if (!elementIds.Contains(elementId))
                    continue;

                Element element = document.GetElement(elementId);
                if (element == null)
                    continue;

                if (!builtInCategory_Tag.IsValidTagCategory((BuiltInCategory)element.Category.Id.IntegerValue))
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

                Reference reference = new Reference(element);
                IndependentTag independentTag = IndependentTag.Create(document, elementId_Tag, view.Id, reference, addLeader, tagOrientation, xyz);
                if (independentTag != null)
                    result.Add(independentTag);
            }

            return result;
        }
    
        public static List<IndependentTag> TagElements(this Document document, IEnumerable<string> templateNames, ElementId elementId_Tag, IEnumerable<ElementId> elementIds, bool addLeader = false, TagOrientation tagOrientation = TagOrientation.Horizontal, IEnumerable<Autodesk.Revit.DB.ViewType> viewTypes = null)
        {
            if (templateNames == null || elementId_Tag == null || elementId_Tag == ElementId.InvalidElementId)
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

                foreach(View view in views_Temp)
                {
                    List<IndependentTag> independentTags = TagElements(view, elementId_Tag, elementIds, addLeader, tagOrientation);
                    if (independentTags != null && independentTags.Count != 0)
                        result.AddRange(independentTags);
                }
            }

            return result;
        }
    }
}