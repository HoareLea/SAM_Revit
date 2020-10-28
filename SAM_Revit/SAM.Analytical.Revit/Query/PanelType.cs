using Autodesk.Revit.DB;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static PanelType PanelType(BuiltInCategory builtInCategory)
        {
            switch (builtInCategory)
            {
                case Autodesk.Revit.DB.BuiltInCategory.OST_Walls:
                    return Analytical.PanelType.Wall;

                case Autodesk.Revit.DB.BuiltInCategory.OST_Roofs:
                    return Analytical.PanelType.Roof;

                case Autodesk.Revit.DB.BuiltInCategory.OST_Floors:
                    return Analytical.PanelType.Floor;

                case Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings:
                    return Analytical.PanelType.Ceiling;
            }

            return Analytical.PanelType.Undefined;
        }
        
        public static PanelType PanelType(this HostObject hostObject)
        {
            if (hostObject == null)
                return Analytical.PanelType.Undefined;

            PanelType panelType = PanelType(hostObject?.Document?.GetElement(hostObject.GetTypeId()) as HostObjAttributes);
            if (panelType != Analytical.PanelType.Undefined)
                return panelType;

            return PanelType((BuiltInCategory)hostObject.Category.Id.IntegerValue);
        }

        public static PanelType PanelType(this HostObjAttributes hostObjAttributes)
        {
            if (hostObjAttributes == null)
                return Analytical.PanelType.Undefined;

            string parameterName_Type = Core.Revit.Query.Name(ActiveSetting.Setting, typeof(Construction), typeof(FamilySymbol), ConstructionParameter.DefaultPanelType);
            if (!string.IsNullOrWhiteSpace(parameterName_Type))
            {
                IEnumerable<Parameter> parameters = hostObjAttributes.GetParameters(parameterName_Type);
                if(parameters != null && parameters.Count() > 0)
                {
                    foreach(Parameter parameter in parameters)
                    {
                        if(parameter != null && parameter.HasValue && parameter.StorageType == StorageType.String)
                        {
                            string text = parameter.AsString();
                            if(!string.IsNullOrWhiteSpace(text))
                            {
                                PanelType result = Analytical.Query.PanelType(text);
                                if (result != Analytical.PanelType.Undefined)
                                    return result;
                            }
                        }
                    }
                }
            }

            return Analytical.Query.PanelType(hostObjAttributes.Name);
        }

        public static PanelType PanelType(this Construction construction)
        {
            if (construction == null)
                return Analytical.PanelType.Undefined;

            PanelType result = Analytical.PanelType.Undefined;

            string text = null;
            if (construction.TryGetValue(ConstructionParameter.DefaultPanelType, out text))
            {
                if (!string.IsNullOrWhiteSpace(text))
                    result = Analytical.Query.PanelType(text);
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