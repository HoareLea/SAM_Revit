using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ElementId ElementId(this SAMObject sAMObject)
        {
            if (sAMObject == null)
                return null;

            if (!sAMObject.TryGetValue(ElementParameter.RevitId, out LongId longId) || longId == null)
            {
                return null;
            }

            return ElementId(longId);
        }

        public static ElementId ElementId(this LongId longId)
        {
            if (longId == null)
            {
                return null;
            }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return new ElementId(System.Convert.ToInt32(longId.Id));
#else
            return new ElementId(longId.Id);
#endif
        }

        public static ElementId ElementId(this string originatingElementDescription)
        {
            if (string.IsNullOrEmpty(originatingElementDescription))
                return null;

            int startIndex = originatingElementDescription.LastIndexOf("[");
            if (startIndex == -1)
                return null;

            int endIndex = originatingElementDescription.IndexOf("]", startIndex);
            if (endIndex == -1)
                return null;

            string elementID = originatingElementDescription.Substring(startIndex + 1, endIndex - startIndex - 1);

            int id;
            if (!int.TryParse(elementID, out id))
                return null;

            return new ElementId(id);
        }
    }
}