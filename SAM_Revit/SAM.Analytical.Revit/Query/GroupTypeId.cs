using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024

#else
        public static ForgeTypeId GroupTypeId(this string builtInParameterGroupText)
        {
            if(string.IsNullOrWhiteSpace(builtInParameterGroupText))
            {
                return null;
            }


            string value = builtInParameterGroupText;
            if(value.StartsWith("PG_"))
            {
                value = value.Substring(2);
            }

            value = value.ToUpper().Trim();

            value = value.Replace(" ", "_");

            switch(value)
            {
                case "ENERGY_ANALYSIS":
                    return Autodesk.Revit.DB.GroupTypeId.EnergyAnalysis;

                case "SUPPORT":
                    return Autodesk.Revit.DB.GroupTypeId.Support;

                case "ANALYSIS_RESULTS":
                    return Autodesk.Revit.DB.GroupTypeId.AnalysisResults;

                case "IDENTITY_DATA":
                    return Autodesk.Revit.DB.GroupTypeId.IdentityData;

                case "GEOMETRY":
                    return Autodesk.Revit.DB.GroupTypeId.Geometry;

                case "ELECTRICAL":
                    return Autodesk.Revit.DB.GroupTypeId.Electrical;

                case "FIRE_PROTECTION":
                    return Autodesk.Revit.DB.GroupTypeId.FireProtection;

                case "ELECTRICAL_LIGHTING":
                    return Autodesk.Revit.DB.GroupTypeId.ElectricalLighting;

                case "DATA":
                    return Autodesk.Revit.DB.GroupTypeId.Data;

                default:
                    return Autodesk.Revit.DB.GroupTypeId.Data;
            }

            return null;
        }
#endif
    }
}