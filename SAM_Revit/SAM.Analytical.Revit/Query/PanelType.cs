using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static PanelType PanelType(this Autodesk.Revit.DB.HostObject hostObject)
        {
            switch((Autodesk.Revit.DB.BuiltInCategory)hostObject.Category.Id.IntegerValue)
            {
                case Autodesk.Revit.DB.BuiltInCategory.OST_Walls:
                    return Analytical.PanelType.Wall;
                case Autodesk.Revit.DB.BuiltInCategory.OST_Roofs:
                    return Analytical.PanelType.Roof;
                case Autodesk.Revit.DB.BuiltInCategory.OST_Floors:
                    return Analytical.PanelType.Floor;
            }

            return Analytical.PanelType.Undefined;
        }

    }
}
