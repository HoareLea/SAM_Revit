using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Create
    {
        public static Log Log(SAMObject sAMObject, Document document)
        {
            if (sAMObject == null || document == null)
                return null;

            ElementId elementId = Query.ElementId(sAMObject);
            if (elementId == null || elementId == ElementId.InvalidElementId)
                return null;

            Log result = new Log();

            Element element = document.GetElement(elementId);
            if (element == null)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                result.Add("There is no mathing Revit element with Element Id {0} for {1} {2} (Guid: {3})", LogRecordType.Warning, elementId.IntegerValue, sAMObject.Name, sAMObject.GetType().Name, sAMObject.Guid);
#else
                result.Add("There is no mathing Revit element with Element Id {0} for {1} {2} (Guid: {3})", LogRecordType.Warning, elementId.Value, sAMObject.Name, sAMObject.GetType().Name, sAMObject.Guid);
#endif
                return result;
            }

            return result;
        }
    }
}