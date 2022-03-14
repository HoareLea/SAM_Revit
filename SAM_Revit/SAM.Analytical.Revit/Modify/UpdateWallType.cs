using Autodesk.Revit.DB;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static Autodesk.Revit.DB.WallType UpdateWallType(this Document document, Autodesk.Revit.DB.Wall wall)
        {
            List<Autodesk.Revit.DB.WallType> wallTypes = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Walls).OfClass(typeof(Autodesk.Revit.DB.WallType)).Cast<Autodesk.Revit.DB.WallType>().ToList();
            if (wallTypes == null || wallTypes.Count < 1)
                return null;

            Autodesk.Revit.DB.WallType wallType = null;
            foreach (Autodesk.Revit.DB.WallType wallType_Temp in wallTypes)
            {
                Parameter parameter = wallType_Temp.LookupParameter("SAM_BuildingElementDescription");
                if (parameter != null)
                {
                    string value = parameter.AsString();
                    if (!string.IsNullOrEmpty(value) && value == wall.Name)
                    {
                        wallType = wallType_Temp;
                        break;
                    }
                }
            }

            if (wallType == null)
            {
                string name = string.Format("ARCH_Wall_{0}", wall.Name);
                wallType = wallTypes.Find(x => x.Name == name);
                if (wallType == null)
                {
                    wallType = wallTypes.Find(x => x.Name == "ARCH_Wall");
                    if (wallType == null)
                    {
                        wallType = wallTypes.First();
                        wallType = wallType.Duplicate(name) as Autodesk.Revit.DB.WallType;
                    }
                }

                if (wallType == null)
                    return null;

                Element element = wall.Document.GetElement(wall.GetTypeId());
                if (element == null)
                    return wallType;

                Parameter parameter_Source = null;
                Parameter parameter_Destination = null;

                parameter_Source = element.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM);
                if (parameter_Source != null)
                {
                    parameter_Destination = wallType.LookupParameter("SAM_BuildingElementThickness");
                    if (parameter_Destination != null)
                        parameter_Destination.Set(parameter_Source.AsDouble());
                }

                parameter_Source = element.get_Parameter(BuiltInParameter.FUNCTION_PARAM);
                if (parameter_Source != null)
                {
                    parameter_Destination = wallType.LookupParameter("SAM_BuildingElementDescription");
                    if (parameter_Destination != null)
                        parameter_Destination.Set(name);

                    parameter_Destination = wallType.get_Parameter(BuiltInParameter.FUNCTION_PARAM);
                    if (parameter_Destination != null)
                        parameter_Destination.Set(parameter_Destination.AsInteger());
                }
            }

            return wallType;

        }
    }
}