using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static LongId ToSAM(this ElementId elementId)
        {
            if (elementId == null)
                return null;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return new LongId(System.Convert.ToInt64(elementId.IntegerValue));
#else
            return new LongId(elementId.Value);
#endif
            
        }
    }
}