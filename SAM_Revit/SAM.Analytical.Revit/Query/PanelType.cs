using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static PanelType PanelType(this Autodesk.Revit.DB.HostObject hostObject)
        {
            switch ((Autodesk.Revit.DB.BuiltInCategory)hostObject.Category.Id.IntegerValue)
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

        public static PanelType PanelType(this Construction construction)
        {
            if (construction == null)
                return Analytical.PanelType.Undefined;

            PanelType result = Analytical.PanelType.Undefined;

            string parameterName_PanelType = Query.ParameterName_PanelType();
            if (!string.IsNullOrWhiteSpace(parameterName_PanelType))
            {
                string text = null;
                if(construction.TryGetValue(parameterName_PanelType, out text))
                {
                    if(!string.IsNullOrWhiteSpace(text))
                        result = Analytical.Query.PanelType(text);
                }
            }

            if (result != Analytical.PanelType.Undefined)
                return result;

            PanelType panelType_Temp = Analytical.Query.PanelType(construction?.Name);
            if (panelType_Temp != Analytical.PanelType.Undefined)
                result = panelType_Temp;

            return result;
        }
    }
}