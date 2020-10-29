using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostObjAttributes hostObjAttributes, Core.Revit.ConvertSettings convertSettings)
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
                result = new Construction(construction);
            else
                result = new Construction(hostObjAttributes.Name);

            result.UpdateParameterSets(hostObjAttributes, ActiveSetting.Setting.GetValue<Core.MapCluster>(Core.Revit.ActiveSetting.Name.ParameterMap));
            
            //result.Add(Core.Revit.Query.ParameterSet(hostObjAttributes));

            convertSettings?.Add(hostObjAttributes.Id, result);

            if (panelType != PanelType.Undefined)
                result.SetValue(ConstructionParameter.DefaultPanelType, panelType.Text());
            else
                result.SetValue(ConstructionParameter.DefaultPanelType, null);

            return result;
        }
    }
}