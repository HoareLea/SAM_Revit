using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static IEnumerable<Core.SAMObject> ToSAM(this Element element)
        {
            IEnumerable<Core.SAMObject> result = null;
            if (element is Wall || element is Floor || element is RoofBase)
            {
                List<Panel> panels = ToSAM((HostObject)element);
                if (panels != null)
                    result = panels.Cast<Core.SAMObject>();
            }
            else if (element is HostObjAttributes)
            {
                Construction construction = ToSAM((HostObjAttributes)element);
                if (construction != null)
                    result = new List<Core.SAMObject>() { construction };
            }
            else if (element is SpatialElement)
            {
                Space space = ToSAM((SpatialElement)element);
                if (space != null)
                    result = new List<Core.SAMObject>() { space };
            }
            else if (element is FamilyInstance)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    FamilyInstance familyInstance = (FamilyInstance)element;

                    //PanelType panelType = PanelType.Undefined;

                    //Panel panel = (familyInstance.Host as HostObject)?.ToSAM()?.First();
                    //if (panel != null)
                    //    panelType = panel.PanelType;

                    //Aperture aperture = ToSAM_Aperture(familyInstance, panelType);
                    Aperture aperture = ToSAM_Aperture(familyInstance);
                    if (aperture != null)
                        result = new List<Core.SAMObject>() { aperture };
                }
            }
            else if (element is FamilySymbol)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction((FamilySymbol)element);
                    if (apertureConstruction != null)
                        result = new List<Core.SAMObject>() { apertureConstruction };
                }
            }

            return result;
        }
    }
}