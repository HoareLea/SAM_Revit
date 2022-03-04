using Autodesk.Revit.DB;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static bool CopySpatialElementParameters(this Document document, Tool tool)
        {
            if(document == null || tool == Tool.Undefined)
            {
                return false;
            }

            DelimitedFileTable delimitedFileTable = Query.DefaultToolsParameterMap();
            if(delimitedFileTable == null)
            {
                return false;
            }

            string source = null;
            switch (tool)
            {
                case Tool.EnergyPlus:
                    source = "E";
                    break;

                case Tool.IES:
                    source = "IES";
                    break;

                case Tool.TAS:
                    source = "TAS";
                    break;

                case Tool.Other:
                    source = "X";
                    break;
            }

            int index_Source = delimitedFileTable.GetColumnIndex(source);
            int index_Destination = delimitedFileTable.GetColumnIndex("Generic");
            if (index_Source == -1 || index_Destination == -1)
            {
                return false;
            }

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int i = 0; i < delimitedFileTable.RowCount; i++)
            {
                string name_Source = delimitedFileTable[i, index_Source]?.ToString();
                if (string.IsNullOrWhiteSpace(name_Source))
                {
                    continue;
                }

                string name_Destination = delimitedFileTable[i, index_Destination]?.ToString();
                if (string.IsNullOrWhiteSpace(name_Destination))
                {
                    continue;
                }

                dictionary[name_Destination] = name_Source;
            }

            if (dictionary != null || dictionary.Count != 0)
            {
                List<SpatialElement> spatialElements = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<SpatialElement>().ToList();
                if (spatialElements != null && spatialElements.Count != 0)
                {
                    foreach (SpatialElement spatialElement in spatialElements)
                    {
                        if (spatialElement == null)
                        {
                            continue;
                        }

                        foreach (KeyValuePair<string, string> keyValuePair in dictionary)
                        {
                            Core.Revit.Modify.CopyValue(spatialElement.LookupParameter(keyValuePair.Value), spatialElement.LookupParameter(keyValuePair.Key));
                        }
                    }

                }
            }

            return true;
        }

    }
}