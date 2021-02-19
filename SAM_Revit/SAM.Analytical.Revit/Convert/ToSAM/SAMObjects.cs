using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static IEnumerable<Core.SAMObject> ToSAM(this Element element, Core.Revit.ConvertSettings convertSettings)
        {
            IEnumerable<Core.SAMObject> result = null;
            if (element is Wall || element is Floor || element is RoofBase)
            {
                List<Panel> panels = ToSAM((HostObject)element, convertSettings);
                if (panels != null)
                    result = panels.Cast<Core.SAMObject>();
            }
            else if (element is HostObjAttributes)
            {
                Construction construction = ToSAM((HostObjAttributes)element, convertSettings);
                if (construction != null)
                    result = new List<Core.SAMObject>() { construction };
            }
            else if (element is SpatialElement)
            {
                Space space = ToSAM((SpatialElement)element, convertSettings);
                if (space != null)
                    result = new List<Core.SAMObject>() { space };
            }
            else if (element is FamilyInstance)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    FamilyInstance familyInstance = (FamilyInstance)element;

                    Aperture aperture = ToSAM_Aperture(familyInstance, convertSettings);
                    if (aperture != null)
                        result = new List<Core.SAMObject>() { aperture };
                }
            }
            else if (element is FamilySymbol)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction((FamilySymbol)element, convertSettings);
                    if (apertureConstruction != null)
                        result = new List<Core.SAMObject>() { apertureConstruction };
                }
            }
            else if (element is ModelCurve)
            {
                if(element.Category != null && (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MEPSpaceSeparationLines || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines))
                {
                    List<Panel> panels = ToSAM_Panels((ModelCurve)element, convertSettings);
                    if (panels != null)
                        result = panels.ConvertAll(x => x as Core.SAMObject);
                }
            }
            return result;
        }
    }
}