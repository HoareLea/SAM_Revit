using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static ElementId ToRevit(this LongId longId)
        {
            if (longId == null)
                return null;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return new ElementId(System.Convert.ToInt32(longId.Id));
#else
            return new ElementId(longId.Id);
#endif
        }
    }
}