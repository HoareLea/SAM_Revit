using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static FilteredElementCollector FilteredElementCollector(this Document document, Type type)
        {
            if (type == null || document == null)
                return null;

            if (type == typeof(Panel))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Walls, Autodesk.Revit.DB.BuiltInCategory.OST_Floors, Autodesk.Revit.DB.BuiltInCategory.OST_Roofs };
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsNotElementType();
            }

            if (type == typeof(Construction))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Walls, Autodesk.Revit.DB.BuiltInCategory.OST_Floors, Autodesk.Revit.DB.BuiltInCategory.OST_Roofs };
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsElementType();
            }

            if (type == typeof(Aperture))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Windows, Autodesk.Revit.DB.BuiltInCategory.OST_Doors };
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsNotElementType();
            }

            if (type == typeof(ApertureConstruction))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Windows, Autodesk.Revit.DB.BuiltInCategory.OST_Doors };
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsElementType();
            }

            if(type == typeof(Space))
            {
                FilteredElementCollector fileteredElementCollector = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaces);
                IEnumerable<ElementId> elementIds = fileteredElementCollector.ToElementIds();
                if(elementIds == null || elementIds.Count() == 0)
                    fileteredElementCollector = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Rooms);

                return fileteredElementCollector;
            }

            return null;
        }

        public static FilteredElementCollector FilteredElementCollector(this Document document, PanelType panelType)
        {
            if (document == null || panelType == Analytical.PanelType.Undefined)
                return null;

            List<BuiltInCategory> builtInCategories = panelType.BuiltInCategories();
            if (builtInCategories == null || builtInCategories.Count == 0)
                return null;

            builtInCategories.RemoveAll(x => x == Autodesk.Revit.DB.BuiltInCategory.INVALID);
            if (builtInCategories.Count == 0)
                return null;

            if (builtInCategories.Count == 1)
                return new FilteredElementCollector(document).OfCategory(builtInCategories[0]);

            return new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));
        }
    }
}