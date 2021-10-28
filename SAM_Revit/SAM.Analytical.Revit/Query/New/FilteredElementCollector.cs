using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static FilteredElementCollector FilteredElementCollector_New(this Document document, Type type)
        {
            if (type == null || document == null)
                return null;

            if(type == typeof(Wall))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Walls).WhereElementIsNotElementType();
            }

            if (type == typeof(WallType))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Walls).WhereElementIsElementType();
            }

            if (type == typeof(Floor))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Floors).WhereElementIsNotElementType();
            }

            if (type == typeof(FloorType))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Floors).WhereElementIsElementType();
            }

            if (type == typeof(Roof))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs).WhereElementIsNotElementType();
            }

            if (type == typeof(RoofType))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs).WhereElementIsElementType();
            }

            if (type == typeof(Door))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Doors).WhereElementIsNotElementType();
            }

            if (type == typeof(DoorType))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Doors).WhereElementIsElementType();
            }

            if (type == typeof(Window))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Windows).WhereElementIsNotElementType();
            }

            if (type == typeof(WindowType))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Windows).WhereElementIsElementType();
            }

            if(type == typeof(Space))
            {
                FilteredElementCollector fileteredElementCollector = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaces);
                IEnumerable<ElementId> elementIds = fileteredElementCollector.ToElementIds();
                if(elementIds == null || elementIds.Count() == 0)
                    fileteredElementCollector = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Rooms);

                return fileteredElementCollector;
            }

            if (type.IsAssignableFrom(typeof(Core.IMaterial)))
            {
                return new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Materials);
            }

            return null;
        }
    }
}