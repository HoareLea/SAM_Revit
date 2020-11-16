using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static BuiltInCategory BuiltInCategory(this PanelType panelType)
        {
            List<BuiltInCategory> result = BuiltInCategories(panelType);
            if (result == null || result.Count != 1)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            return result[0];
        }

        public static BuiltInCategory BuiltInCategory(this Geometry.Spatial.Vector3D normal)
        {
            PanelType panelType = Analytical.Query.PanelType(normal, Core.Revit.Tolerance.Tilt);
            if (panelType == Analytical.PanelType.Undefined)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            return BuiltInCategory(panelType);
        }

        public static BuiltInCategory BuiltInCategory(this ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            return BuiltInCategory(apertureConstruction.ApertureType);
        }

        public static BuiltInCategory BuiltInCategory(this ApertureType apertureType)
        {
            switch(apertureType)
            {
                case Analytical.ApertureType.Door:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Doors;
                case Analytical.ApertureType.Window:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Windows;
            }

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }
    }
}