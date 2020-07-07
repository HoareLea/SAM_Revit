using Autodesk.Revit.DB;
using NetTopologySuite.Mathematics;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static BuiltInCategory BuiltInCategory(this PanelType panelType)
        {
            switch (panelType)
            {
                case Analytical.PanelType.Roof:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Roofs;

                case Analytical.PanelType.Wall:
                case Analytical.PanelType.WallExternal:
                case Analytical.PanelType.WallInternal:
                case Analytical.PanelType.UndergroundWall:
                case Analytical.PanelType.CurtainWall:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Walls;

                case Analytical.PanelType.Floor:
                case Analytical.PanelType.FloorExposed:
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.FloorRaised:
                case Analytical.PanelType.SlabOnGrade:
                case Analytical.PanelType.UndergroundSlab:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Floors;

                case Analytical.PanelType.Ceiling:
                case Analytical.PanelType.UndergroundCeiling:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings;
            }

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }

        public static BuiltInCategory BuiltInCategory(this Geometry.Spatial.Vector3D normal)
        {
            PanelType panelType = Analytical.Query.PanelType(normal);
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