using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<IAnalyticalObject> ToSAM_AnalyticalObject(this Element element, ConvertSettings convertSettings)
        {
            List<IAnalyticalObject> result = null;
            if (element is WallSweep)
            {
                List<IPartition> partitions = ToSAM_Partitions((WallSweep)element, convertSettings);
                if (partitions != null)
                    result = partitions.ConvertAll(x => x as IAnalyticalObject);
            }
            else if (element is HostObject)
            {
                List<IPartition> partitions = ToSAM_Partitions((HostObject)element, convertSettings);
                if (partitions != null)
                    result = partitions.ConvertAll(x => x as IAnalyticalObject);
            }
            else if (element is HostObjAttributes)
            {
                HostPartitionType hostPartitionType = ToSAM_HostPartitionType((HostObjAttributes)element, convertSettings);
                if (hostPartitionType != null)
                    result = new List<IAnalyticalObject>() { hostPartitionType };
            }
            else if (element is SpatialElement)
            {
                Space space = ToSAM((SpatialElement)element, convertSettings);
                if (space != null)
                    result = new List<IAnalyticalObject>() { space };
            }
            else if (element is FamilyInstance)
            {
                FamilyInstance familyInstance = (FamilyInstance)element;

                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    IOpening opening = ToSAM_Opening(familyInstance, convertSettings);
                    if (opening != null)
                        result = new List<IAnalyticalObject>() { opening };
                }
                else
                {
                    //result = familyInstance.ToSAM_Panels(convertSettings);
                }
            }
            else if (element is FamilySymbol)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                {
                    OpeningType openingType = ToSAM_OpeningType((FamilySymbol)element, convertSettings);
                    if (openingType != null)
                        result = new List<IAnalyticalObject>() { openingType };
                }
            }
            else if (element is ModelCurve)
            {
                //if (element.Category != null && (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MEPSpaceSeparationLines || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines))
                //{
                //    List<Panel> panels = ToSAM_Panels((ModelCurve)element, convertSettings);
                //    if (panels != null)
                //        result = panels.ConvertAll(x => x as Core.SAMObject);
                //}
            }
            return result;
        }

        public static List<T> ToSAM_AnalyticalObject<T>(this RevitLinkInstance revitLinkInstance, ConvertSettings convertSettings) where T : IAnalyticalObject
        {
            return ToSAM_AnalyticalObject(revitLinkInstance, typeof(T), convertSettings)?.Cast<T>()?.ToList();
        }

        public static List<IAnalyticalObject> ToSAM_AnalyticalObject(this RevitLinkInstance revitLinkInstance, System.Type type, ConvertSettings convertSettings)
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

            return ToSAM_AnalyticalObject(document, type, convertSettings, transform);
        }

        public static List<IAnalyticalObject> ToSAM_AnalyticalObject(this Document document, System.Type type, ConvertSettings convertSettings, Transform transform = null)
        {
            if (document == null || type == null)
                return null;

            if (transform == null)
                transform = Transform.Identity;

            List<Element> elements = Query.FilteredElementCollector_New(document, type)?.ToList();
            if (elements == null)
                return null;

            List<IAnalyticalObject> result = new List<IAnalyticalObject>();
            for (int i = 0; i < elements.Count; i++)
            {
                IEnumerable<IAnalyticalObject> analyticalObjects = ToSAM_AnalyticalObject(elements[i], convertSettings);
                if (analyticalObjects == null || analyticalObjects.Count() == 0)
                    continue;

                foreach (IAnalyticalObject analyticalObject in analyticalObjects)
                    result.Add(analyticalObject);
            };

            if (transform != null && transform != Transform.Identity)
            {
                Parallel.For(0, result.Count, (int i) =>
                {
                    result[i] = Query.Transform(transform, result[i]);
                });
            }

            return result;
        }

        public static List<IAnalyticalObject> ToSAM_AnalyticalObject(this Document document, ConvertSettings convertSettings)
        {
            if (document == null)
                return null;

            return ToSAM_AnalyticalObject(document, null, convertSettings);
        }

    }
}