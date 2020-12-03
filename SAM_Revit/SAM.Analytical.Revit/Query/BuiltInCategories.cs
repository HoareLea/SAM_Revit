using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static List<BuiltInCategory> BuiltInCategories(this PanelType panelType)
        {

            List<BuiltInCategory> result = new List<BuiltInCategory>();
            switch (panelType)
            {
                case Analytical.PanelType.Roof:
                case Analytical.PanelType.UndergroundCeiling:
                    result.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
                    break;

                case Analytical.PanelType.Wall:
                case Analytical.PanelType.WallExternal:
                case Analytical.PanelType.WallInternal:
                case Analytical.PanelType.UndergroundWall:
                case Analytical.PanelType.CurtainWall:
                    result.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
                    break;

                case Analytical.PanelType.Floor:
                case Analytical.PanelType.FloorExposed:
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.FloorRaised:
                case Analytical.PanelType.SlabOnGrade:
                case Analytical.PanelType.UndergroundSlab:
                    result.Add( Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                    break;

                case Analytical.PanelType.Ceiling:
                    result.Add( Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings);
                    break;

                case Analytical.PanelType.Shade:
                    result.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
                    result.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
                    break;
                case Analytical.PanelType.Air:
                    result.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                    result.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
                    break;

                default:
                    return null;
            }

            return result;

        }
    }
}