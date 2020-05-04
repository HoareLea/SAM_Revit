using Autodesk.Revit.DB;

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
    }
}