using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static ApertureType ApertureType(this Element element)
        {

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
           switch ((BuiltInCategory)element.Category.Id.IntegerValue)
#else
            switch ((BuiltInCategory)element.Category.Id.Value)
#endif
            {
                case Autodesk.Revit.DB.BuiltInCategory.OST_Windows:
                case Autodesk.Revit.DB.BuiltInCategory.OST_CurtainWallPanels:
                    return Analytical.ApertureType.Window;

                case Autodesk.Revit.DB.BuiltInCategory.OST_Doors:
                    return Analytical.ApertureType.Door;
            }

            return Analytical.ApertureType.Undefined;
        }
    }
}