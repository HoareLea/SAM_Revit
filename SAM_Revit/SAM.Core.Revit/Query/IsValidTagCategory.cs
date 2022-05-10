using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool IsValidTagCategory(this BuiltInCategory builtInCategory_Tag, BuiltInCategory builtInCategory_Element)
        {
            if (builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.INVALID || builtInCategory_Tag == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                return false;

            if (builtInCategory_Tag == Autodesk.Revit.DB.BuiltInCategory.OST_MultiCategoryTags)
                return true;

            switch(builtInCategory_Tag)
            {
                case Autodesk.Revit.DB.BuiltInCategory.OST_AssemblyTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_Assemblies;
                case Autodesk.Revit.DB.BuiltInCategory.OST_WallTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_Walls;
                case Autodesk.Revit.DB.BuiltInCategory.OST_FloorTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_Floors;
                case Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaceTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_MEPSpaces;
                case Autodesk.Revit.DB.BuiltInCategory.OST_WindowTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_Windows;
                case Autodesk.Revit.DB.BuiltInCategory.OST_DoorTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_Doors;
                case Autodesk.Revit.DB.BuiltInCategory.OST_RoofTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_Roofs;
                case Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipmentTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_MechanicalEquipment;
                case Autodesk.Revit.DB.BuiltInCategory.OST_DuctTerminalTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_DuctTerminal;
                case Autodesk.Revit.DB.BuiltInCategory.OST_PipeFittingTags:
                    return builtInCategory_Element == Autodesk.Revit.DB.BuiltInCategory.OST_PipeFitting;

            }

            return false;
        }
    }
}