using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace SAM.Analytical.Revit
{
    public class RevitLinkInstanceSelectionFilter : ISelectionFilter
    {
        private BuiltInCategory builtInCategory;
        private Document document;

        public RevitLinkInstanceSelectionFilter(Document document, BuiltInCategory builtInCategory)
        {
            this.document = document;
            this.builtInCategory = builtInCategory;
        }

        public bool AllowElement(Element element)
        {
            if (element == null || element.Category == null)
                return false;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RvtLinks)
                return true;
            else
                return element.Category.Id.IntegerValue == (int)builtInCategory;
#else
            if (element.Category.Id.Value == (long)BuiltInCategory.OST_RvtLinks)
                return true;
            else
                return element.Category.Id.Value == (long)builtInCategory;
#endif

        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (reference.ElementId == null || reference.ElementId == ElementId.InvalidElementId)
                return false;

            if (reference.LinkedElementId == null || reference.LinkedElementId == ElementId.InvalidElementId)
                return false;

            RevitLinkInstance revitLinkInstance = this.document.GetElement(reference.ElementId) as RevitLinkInstance;

            if (revitLinkInstance == null)
                return false;

            Document document = revitLinkInstance.GetLinkDocument();
            if (document == null)
                return false;

            Element element = document.GetElement(reference.LinkedElementId);
            if (element == null)
                return false;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return element.Category.Id.IntegerValue == (int)builtInCategory;
#else
            return element.Category.Id.Value == (long)builtInCategory;
#endif

        }
    }
}
