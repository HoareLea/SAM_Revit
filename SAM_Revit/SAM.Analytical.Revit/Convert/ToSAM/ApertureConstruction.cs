using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static ApertureConstruction ToSAM_ApertureConstruction(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null)
                return null;

            FamilySymbol familySymbol = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as FamilySymbol;
            if (familySymbol == null)
                return null;

            return ToSAM_ApertureConstruction(familySymbol, convertSettings, Query.PanelType((HostObject)familyInstance.Host));
        }

        public static ApertureConstruction ToSAM_ApertureConstruction(this FamilySymbol familySymbol, ConvertSettings convertSettings, PanelType panelType = PanelType.Undefined)
        {
            if (familySymbol == null)
                return null;

            ApertureConstruction result = convertSettings?.GetObject<ApertureConstruction>(familySymbol.Id);
            if (result != null)
                return result;

            string name = familySymbol.FullName();

            ApertureType apertureType = familySymbol.ApertureType();

            List<ApertureConstruction> apertureConstructions = Analytical.ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary).GetApertureConstructions(apertureType);
            if (apertureConstructions != null)
            {
                apertureConstructions = apertureConstructions.FindAll(x => name.Equals(x.UniqueName()) || name.Equals(x.Name) || x.FullName().Equals(name));
                if(apertureConstructions != null && apertureConstructions.Count != 0)
                {
                    if(apertureConstructions.Count > 1 && panelType != PanelType.Undefined)
                    {
                        result = apertureConstructions.Find(x => x.PanelType() == panelType);
                    }

                    if(result == null)
                    {
                        result = apertureConstructions[0];
                    }
                }
            }

            if (result == null)
                result = new ApertureConstruction(name, apertureType);

            result.UpdateParameterSets(familySymbol, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

            convertSettings?.Add(familySymbol.Id, result);

            return result;
        }
    }
}