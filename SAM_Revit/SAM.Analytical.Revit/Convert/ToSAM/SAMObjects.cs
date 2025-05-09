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
            if (element is WallSweep)
            {
                List<Panel> panels = ToSAM_Panels((WallSweep)element, convertSettings);
                if (panels != null)
                    result = panels.ConvertAll(x => x as Core.SAMObject);
            }
            else if (element is HostObject)
            {
                List<Panel> panels = ToSAM((HostObject)element, convertSettings);
                if (panels != null)
                {
                    result = panels.Cast<Core.SAMObject>();
                }
                    
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
                FamilyInstance familyInstance = (FamilyInstance)element;


#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
#else
                if (element.Category.Id.Value == (long)BuiltInCategory.OST_Windows || element.Category.Id.Value == (long)BuiltInCategory.OST_Doors)
#endif
                {
                    List<Aperture> apertures = ToSAM_Apertures(familyInstance, convertSettings);
                    if (apertures != null)
                        result = apertures.Cast<Core.SAMObject>();
                }
                else
                {
                    result = familyInstance.ToSAM_Panels(convertSettings);
                }
            }
            else if (element is FamilySymbol)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors  || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CurtainWallPanels)
#else
                if (element.Category.Id.Value == (long)BuiltInCategory.OST_Windows || element.Category.Id.Value == (long)BuiltInCategory.OST_Doors || element.Category.Id.Value == (long)BuiltInCategory.OST_CurtainWallPanels)
#endif
                {
                    ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction((FamilySymbol)element, convertSettings);
                    if (apertureConstruction != null)
                        result = new List<Core.SAMObject>() { apertureConstruction };
                }
            }
            else if (element is ModelCurve)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                if (element.Category != null && (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MEPSpaceSeparationLines || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines))
#else
                if (element.Category != null && (element.Category.Id.Value == (long)BuiltInCategory.OST_MEPSpaceSeparationLines || element.Category.Id.Value == (long)BuiltInCategory.OST_RoomSeparationLines))
#endif
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
    
        public static IEnumerable<Core.SAMObject> ToSAM(this Document document, System.Type type, Core.Revit.ConvertSettings convertSettings, Transform transform = null, Phase phase = null)
        {
            if (document == null || type == null)
                return null;

            if (transform == null)
                transform = Transform.Identity;

            FilteredElementCollector filteredElementCollector = Query.FilteredElementCollector(document, type);

            List<Element> elements = filteredElementCollector?.ToList();
            if (elements == null)
                return null;

            if(phase != null)
            {
                for (int i = elements.Count - 1; i >= 0; i--)
                {
                    ElementOnPhaseStatus elementOnPhaseStatus = elements[i].GetPhaseStatus(phase.Id);
                    if(elementOnPhaseStatus == ElementOnPhaseStatus.Past || elementOnPhaseStatus == ElementOnPhaseStatus.Demolished || elementOnPhaseStatus == ElementOnPhaseStatus.Future)
                    {
                        elements.RemoveAt(i);
                    }
                }
            }

            List<Core.SAMObject> result = new List<Core.SAMObject>();
            for (int i = 0; i < elements.Count; i++)
            {
                IEnumerable<Core.SAMObject> sAMObjects = ToSAM(elements[i], convertSettings);
                if (sAMObjects == null || sAMObjects.Count() == 0)
                    continue;

                foreach (Core.SAMObject sAMObject in sAMObjects)
                    result.Add(sAMObject);
            };

            if(transform != null && transform != Transform.Identity)
            {
                Parallel.For(0, result.Count, (int i) =>
                {
                    result[i] = Query.Transform(transform, result[i]);
                });
            }

            return result;
        }

        public static IEnumerable<T> ToSAM<T>(this Document document, Core.Revit.ConvertSettings convertSettings, Transform transform = null)
        {
            IEnumerable<Core.SAMObject> sAMObjects = ToSAM(document, typeof(T), convertSettings, transform);
            if(sAMObjects == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(Core.SAMObject sAMObject in sAMObjects)
            {
                if(sAMObject is T)
                {
                    result.Add((T)(object)sAMObject);
                }
            }

            return result;
        }
    }
}