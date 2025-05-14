using Autodesk.Revit.DB;
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

                case Autodesk.Revit.DB.BuiltInCategory.OST_CurtaSystem:
                    return Analytical.PanelType.CurtainWall;

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

            PanelType panelType = PanelType(hostObject.Document?.GetElement(hostObject.GetTypeId()) as HostObjAttributes);
            if (panelType != Analytical.PanelType.Undefined)
                return panelType;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return PanelType((BuiltInCategory)hostObject.Category.Id.IntegerValue);
#else
            return PanelType((BuiltInCategory)hostObject.Category.Id.Value);
#endif

        }

        public static PanelType PanelType(this HostObjAttributes hostObjAttributes)
        {
            if (hostObjAttributes == null)
                return Analytical.PanelType.Undefined;

            string parameterName_Type = Core.Revit.Query.Name(ActiveSetting.Setting, typeof(Construction), typeof(HostObjAttributes), ConstructionParameter.DefaultPanelType);
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
                                PanelType result = Analytical.Query.PanelType(text, false);
                                if (result != Analytical.PanelType.Undefined)
                                    return result;
                            }
                        }
                    }
                }
            }

            return Analytical.Query.PanelType(hostObjAttributes.Name, true);
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

        public static PanelType PanelType(this Geometry.Spatial.Vector3D normal)
        {
            if (normal == null)
                return Analytical.PanelType.Undefined;

            double value;

            value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldZ);
            if (System.Math.Abs(value) <= Core.Revit.Tolerance.Tilt)
                return Analytical.PanelType.Wall;

            if(value < 0)
            {
                value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldY);
                if (System.Math.Abs(value) <= Core.Revit.Tolerance.Tilt)
                    return Analytical.PanelType.Floor;
            }

            return Analytical.PanelType.Roof;
        }
    }
}