using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostObjAttributes hostObjAttributes, ConvertSettings convertSettings)
        {
            if (hostObjAttributes == null)
                return null;

            Construction result = convertSettings?.GetObject<Construction>(hostObjAttributes.Id);
            if (result != null)
                return result;

            string name = hostObjAttributes.Name;
            PanelType panelType = hostObjAttributes.PanelType();
            if (panelType == PanelType.Undefined)
                panelType = Query.PanelType((BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue);

            Construction construction = Analytical.Query.DefaultConstruction(panelType);
            if(construction != null && (name.Equals(construction.Name) || name.Equals(construction.UniqueName())))
            {
                result = new Construction(construction);
            }
            else
            {
                result = new Construction(hostObjAttributes.Name);
            }
                

            result.UpdateParameterSets(hostObjAttributes, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
            
            //result.Add(Core.Revit.Query.ParameterSet(hostObjAttributes));

            convertSettings?.Add(hostObjAttributes.Id, result);

            if (panelType != PanelType.Undefined)
                result.SetValue(ConstructionParameter.DefaultPanelType, panelType.Text());
            else
                result.SetValue(ConstructionParameter.DefaultPanelType, null);

            List<ConstructionLayer> constructionLayers = result.ConstructionLayers;
            if(constructionLayers != null && constructionLayers.Count != 0)
            {
                result.SetValue(ConstructionParameter.DefaultThickness, constructionLayers.ConvertAll(x => x.Thickness).Sum());
            }
            else
            {
                CompoundStructure compoundStructure = hostObjAttributes.GetCompoundStructure();
                if(compoundStructure != null)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    double thickness = UnitUtils.ConvertFromInternalUnits(compoundStructure.GetWidth(), DisplayUnitType.DUT_METERS);
#else
                    double thickness = UnitUtils.ConvertFromInternalUnits(compoundStructure.GetWidth(), UnitTypeId.Meters);
#endif
                    result.SetValue(ConstructionParameter.DefaultThickness, thickness);
                }
            }


            return result;
        }

        public static Construction ToSAM_Construction(this ElementType elementType, ConvertSettings convertSettings)
        {
            if (elementType == null || elementType.Category == null)
            {
                return null;
            }

            if((BuiltInCategory)elementType.Category.Id.IntegerValue != BuiltInCategory.OST_Cornices)
            {
                return null;
            }

            Construction result = convertSettings?.GetObject<Construction>(elementType.Id);
            if (result != null)
                return result;

            PanelType panelType = PanelType.Wall;

            string name = elementType.Name;

            Construction construction = Analytical.Query.DefaultConstruction(panelType);
            if (construction != null && (name.Equals(construction.Name) || name.Equals(construction.UniqueName())))
                result = new Construction(construction);
            else
                result = new Construction(elementType.Name);

            result.UpdateParameterSets(elementType, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
            result.SetValue(ConstructionParameter.DefaultPanelType, panelType.Text());

            convertSettings?.Add(elementType.Id, result);
            return result;

        }

        public static Construction ToSAM_Construction(this RevitType3D revitType3D)
        {
            if(revitType3D == null)
            {
                return null;
            }

            Construction result = new Construction(revitType3D.Guid, revitType3D.Name);

            List<Core.ParameterSet> parameterSets = revitType3D.GetParameterSets();
            if(parameterSets != null)
            {
                foreach(Core.ParameterSet parameterSet in parameterSets)
                {
                    result.Add(parameterSet);
                }
            }

            return result;
        }
    }
}