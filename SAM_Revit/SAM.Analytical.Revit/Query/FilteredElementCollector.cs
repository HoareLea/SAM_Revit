using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static FilteredElementCollector FilteredElementCollector(this Document document, Type type)
        {
            if (type == null || document == null)
                return null;

            if(type == typeof(Panel))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Walls, Autodesk.Revit.DB.BuiltInCategory.OST_Floors, Autodesk.Revit.DB.BuiltInCategory.OST_Roofs};
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsNotElementType();
            }

            if(type == typeof(Construction))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Walls, Autodesk.Revit.DB.BuiltInCategory.OST_Floors, Autodesk.Revit.DB.BuiltInCategory.OST_Roofs };
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsElementType();
            }

            if(type == typeof(Aperture))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Windows, Autodesk.Revit.DB.BuiltInCategory.OST_Doors};
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsNotElementType();
            }

            if(type == typeof(ApertureConstruction))
            {
                List<BuiltInCategory> builtInCategories = new List<BuiltInCategory> { Autodesk.Revit.DB.BuiltInCategory.OST_Windows, Autodesk.Revit.DB.BuiltInCategory.OST_Doors };
                LogicalOrFilter logicalOrFilter = new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter));
                return new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsElementType();
            }

            return null;
        }
    }
}
