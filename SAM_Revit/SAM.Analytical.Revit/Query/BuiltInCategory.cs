using System;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static BuiltInCategory BuiltInCategory(this PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.Roof:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Roofs;
                case PanelType.Wall:
                case PanelType.WallExternal:
                case PanelType.WallInternal:
                case PanelType.UndergroundWall:
                case PanelType.CurtainWall:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Walls;
                case PanelType.Floor:
                case PanelType.FloorExposed:
                case PanelType.FloorInternal:
                case PanelType.FloorRaised:
                case PanelType.SlabOnGrade:
                case PanelType.UndergroundSlab:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Floors;
                case PanelType.Ceiling:
                case PanelType.UndergroundCeiling:
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings;
            }

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }
    }
}
