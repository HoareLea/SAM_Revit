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


                    EnergyAnalysisDetailModelOptions energyAnalysisDetailModelOptions = new EnergyAnalysisDetailModelOptions();
                    energyAnalysisDetailModelOptions.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries;
                    energyAnalysisDetailModelOptions.EnergyModelType = EnergyModelType.SpatialElement;
                    energyAnalysisDetailModelOptions.ExportMullions = true;
                    energyAnalysisDetailModelOptions.IncludeShadingSurfaces = true;
                    energyAnalysisDetailModelOptions.SimplifyCurtainSystems = false;

                    EnergyDataSettings energyDataSettings = EnergyDataSettings.GetFromDocument(document);
                    energyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
                    energyDataSettings.ExportDefaults = false;
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    energyDataSettings.SliverSpaceTolerance = UnitUtils.ConvertToInternalUnits(5, DisplayUnitType.DUT_MILLIMETERS);
#else
                    energyDataSettings.SliverSpaceTolerance = UnitUtils.ConvertToInternalUnits(5, UnitTypeId.Millimeters);
#endif

                    energyDataSettings.AnalysisType = AnalysisMode.BuildingElements;
                    energyDataSettings.EnergyModel = false;

                    energyAnalysisDetailModel = EnergyAnalysisDetailModel.Create(document, energyAnalysisDetailModelOptions);

                    GBXMLExportOptions gBXMLExportOptions = new GBXMLExportOptions();
                    gBXMLExportOptions.ExportEnergyModelType = ExportEnergyModelType.SpatialElement;

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