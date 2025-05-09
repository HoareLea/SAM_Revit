using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static IntegerId ToSAM(this ElementId elementId)
        {
            if (elementId == null)
                return null;
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return new IntegerId(elementId.IntegerValue);
#else
            return new IntegerId(System.Convert.ToInt32(elementId.Value));
#endif

        }
    }
}