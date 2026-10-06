// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static bool TogbXML(this Document document, string path)
        {
            if(document == null || document.IsFamilyDocument || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            bool result = true;

            try
            {
                using (Transaction transaction = new Transaction(document, "Export gbXML"))
                {
                    transaction.Start();

                    EnergyAnalysisDetailModel energyAnalysisDetailModel = null;

                    using (SubTransaction subTransaction = new SubTransaction(document))
                    {
                        subTransaction.Start();
                        energyAnalysisDetailModel = EnergyAnalysisDetailModel.GetMainEnergyAnalysisDetailModel(document);
                        if (energyAnalysisDetailModel != null && energyAnalysisDetailModel.IsValidObject)
                        {
                            document.Delete(energyAnalysisDetailModel.Id);
                        }
                        subTransaction.Commit();
                    }

                    //Reseting Project Base Point
                    IEnumerable<Element> elements = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_ProjectBasePoint);
                    foreach (Element aElement in elements)
                    {
                        if (aElement.Pinned)
                        {
                            aElement.Pinned = false;
                        }

                        Parameter parameter = null;

                        parameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM);
                        if (parameter != null && !parameter.IsReadOnly)
                            parameter.Set(0.0);

                        parameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM);
                        if (parameter != null && !parameter.IsReadOnly)
                            parameter.Set(0.0);

                        parameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                        if (parameter != null && !parameter.IsReadOnly)
                            parameter.Set(0.0);

                        parameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                        if (parameter != null && !parameter.IsReadOnly)
                            parameter.Set(0.0);

                        parameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);
                        if (parameter != null && !parameter.IsReadOnly)
                            parameter.Set(0.0);
                    }


#if Revit2027
                    // EnergyAnalysisDetailModelOptions is deprecated in Revit 2027 and Create(Document, options)
                    // "will be removed in the next version". The model type now comes from Energy Settings alone,
                    // so AnalysisType must be RoomsOrSpaces - the equivalent of the old EnergyModelType.SpatialElement.
                    // The previous AnalysisMode.BuildingElements contradicted the options object and set_AnalysisType
                    // now throws at runtime. ExportMullions, IncludeShadingSurfaces, SimplifyCurtainSystems and Tier
                    // are read-only in 2027 (no API replacement) - the document's Energy Settings values apply.
                    // ExportDefaults is deprecated.
                    EnergyDataSettings energyDataSettings = EnergyDataSettings.GetEnergyDataSettings(document);
                    energyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
                    energyDataSettings.SliverSpaceTolerance = UnitUtils.ConvertToInternalUnits(5, UnitTypeId.Millimeters);
                    energyDataSettings.AnalysisType = AnalysisMode.RoomsOrSpaces;
                    energyDataSettings.EnergyModel = false;

                    energyAnalysisDetailModel = EnergyAnalysisDetailModel.Create(document);
#else
                    EnergyAnalysisDetailModelOptions energyAnalysisDetailModelOptions = new EnergyAnalysisDetailModelOptions();
                    energyAnalysisDetailModelOptions.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries;
                    energyAnalysisDetailModelOptions.EnergyModelType = EnergyModelType.SpatialElement;
                    energyAnalysisDetailModelOptions.ExportMullions = true;
                    energyAnalysisDetailModelOptions.IncludeShadingSurfaces = true;
                    energyAnalysisDetailModelOptions.SimplifyCurtainSystems = false;

                    EnergyDataSettings energyDataSettings = EnergyDataSettings.GetFromDocument(document);
                    energyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
                    energyDataSettings.ExportDefaults = false;
                    energyDataSettings.SliverSpaceTolerance = UnitUtils.ConvertToInternalUnits(5, UnitTypeId.Millimeters);

                    energyDataSettings.AnalysisType = AnalysisMode.BuildingElements;
                    energyDataSettings.EnergyModel = false;

                    energyAnalysisDetailModel = EnergyAnalysisDetailModel.Create(document, energyAnalysisDetailModelOptions);
#endif

                    GBXMLExportOptions gBXMLExportOptions = new GBXMLExportOptions();
#if Revit2025 || Revit2026
                    // GBXMLExportOptions.ExportEnergyModelType, and the ExportEnergyModelType enum it took,
                    // were both removed from the Revit 2027 API - GBXMLExportOptions now exposes only
                    // ExportAnalyticalSystems and ForceGbXMLExport. There is no direct replacement on the
                    // options object; under 2027 the export follows EnergyDataSettings alone, which is
                    // configured above. Whether that yields the same gbXML as this line did is a question for
                    // a model comparison, not something the API can answer.
                    gBXMLExportOptions.ExportEnergyModelType = ExportEnergyModelType.SpatialElement;
#endif

                    if (!document.Export(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileName(path), gBXMLExportOptions))
                    {
                        result = false;
                    }

                    transaction.RollBack();
                }
            }
            catch (System.Exception Exception)
            {
                result = false;
            }

            return result;
        }
    }
}