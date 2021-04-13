using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    
        public static IEnumerable<T> ToSAM<T>(this RevitLinkInstance revitLinkInstance, Core.Revit.ConvertSettings convertSettings) where T: Core.SAMObject
        {
            return ToSAM(revitLinkInstance, typeof(T), convertSettings)?.Cast<T>();
        }

        public static IEnumerable<Core.SAMObject> ToSAM(this RevitLinkInstance revitLinkInstance, System.Type type, Core.Revit.ConvertSettings convertSettings)
        {
            Document document = null;

            try
            {
                document = revitLinkInstance.GetLinkDocument();
            }
            catch
            {
                return null;
            }

            if (document == null)
                return null;

            Transform transform = revitLinkInstance.GetTotalTransform();
            if (transform == null)
                transform = Transform.Identity;

            if (!transform.IsIdentity)
                transform = transform.Inverse;

            return ToSAM(document, type, convertSettings, transform);
        }
    
        public static IEnumerable<Core.SAMObject> ToSAM(this Document document, System.Type type, Core.Revit.ConvertSettings convertSettings, Transform transform = null)
        {
            if (document == null || type == null)
                return null;

            if (transform == null)
                transform = Transform.Identity;

            List<Element> elements = Query.FilteredElementCollector(document, type)?.ToList();
            if (elements == null)
                return null;

            List<List<Core.SAMObject>> sAMObjects = Enumerable.Repeat<List<Core.SAMObject>>(null, elements.Count).ToList();
            Parallel.For(0, elements.Count, (int i) => 
            {
                IEnumerable<Core.SAMObject> sAMObjects_Temp = ToSAM(elements[i], convertSettings);
                if (sAMObjects_Temp == null || sAMObjects_Temp.Count() == 0)
                    return;

                sAMObjects[i] = new List<Core.SAMObject>();
                foreach (Core.SAMObject sAMObject in sAMObjects_Temp)
                    sAMObjects[i].Add(Query.Transform(transform, sAMObject));

            });

            List<Core.SAMObject> result = new List<Core.SAMObject>();
            foreach(List<Core.SAMObject> sAMObjects_Temp in sAMObjects)
            {
                if (sAMObjects_Temp != null && sAMObjects_Temp.Count != 0)
                    result.AddRange(sAMObjects_Temp);
            }

            return result;
        }
    }
}