﻿using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<ViewPlan> DuplicateViewPlan(this ViewPlan viewPlan, IEnumerable<Level> levels = null, bool useExisting = false)
        {
            if (viewPlan == null || !viewPlan.IsValidObject || viewPlan.IsTemplate)
                return null;

            Document document = viewPlan.Document;
            if (document == null)
                return null;

            List<Level> levels_Temp = null;
            if(levels != null)
                levels_Temp = new List<Level>(levels);

            if (levels_Temp == null)
                levels_Temp = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();

            List<Element> elements_ColorFillLegend = new FilteredElementCollector(document, viewPlan.Id).OfCategory(BuiltInCategory.OST_ColorFillLegends).ToList();

            List<ViewPlan> result = new List<ViewPlan>();
            foreach(Level level in levels_Temp)
            {
                if (level.Id == viewPlan.GenLevel.Id)
                    continue;

                ViewPlan viewPlan_New = null;

                List<ViewPlan> viewPlans = new FilteredElementCollector(document).OfClass(typeof(ViewPlan)).Cast<ViewPlan>().ToList();
                string name = level.Name;
                if(useExisting)
                    viewPlan_New = viewPlans.Find(x => x.Name == name);

                if(viewPlan_New == null)
                {
                    viewPlan_New = ViewPlan.Create(document, viewPlan.GetTypeId(), level.Id);

                    // Check name uniqueness
                    string name_Temp = name;
                    int count = 0;
                    while (viewPlans.Find(x => x.Name == name_Temp) != null)
                    {
                        count++;
                        name_Temp = string.Format("{0} {1}", name, count);
                    }

                    viewPlan_New.Name = name_Temp;

                }
                
                foreach(Parameter parameter in viewPlan.ParametersMap)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                    if (parameter.Id.IntegerValue == (int)BuiltInParameter.VIEW_NAME)
                        continue;
#else
                    if (parameter.Id.Value == (long)BuiltInParameter.VIEW_NAME)
                        continue;
#endif



                    Definition definition = parameter?.Definition;
                    if (definition == null)
                        continue;

                    Parameter parameter_New = viewPlan_New.get_Parameter(parameter.Definition);
                    if (parameter_New == null)
                        continue;

                    CopyValue(parameter, parameter_New);
                }

                if(elements_ColorFillLegend != null && elements_ColorFillLegend.Count != 0)
                {
                    ElementTransformUtils.CopyElements(viewPlan, elements_ColorFillLegend.ConvertAll(x => x.Id), viewPlan_New, null, new CopyPasteOptions());
                }

                result.Add(viewPlan_New);

                IEnumerable<ElementId> elementIds_Dependent = viewPlan.GetDependentViewIds();
                if(elementIds_Dependent != null && elementIds_Dependent.Count() != 0)
                {
                    foreach(ElementId elementId_Dependent in elementIds_Dependent)
                    {
                        ViewPlan viewPlan_Dependent = document.GetElement(elementId_Dependent) as ViewPlan;
                        if (viewPlan_Dependent == null)
                            continue;

                        viewPlans = new FilteredElementCollector(document).OfClass(typeof(ViewPlan)).Cast<ViewPlan>().ToList();
                        string name_Dependent = name;
                        if (viewPlan_Dependent.Name.StartsWith(viewPlan.Name))
                        {
                            name_Dependent = level.Name + viewPlan_Dependent.Name.Substring(viewPlan.Name.Length);
                        }
                        else
                        {
                            Element scopeBox = viewPlan_Dependent.ScopeBox();
                            if (scopeBox != null)
                                name_Dependent = level.Name + " - " + scopeBox.Name;
                        }

                        ViewPlan viewPlan_Dependent_New = null;

                        if (useExisting)
                            viewPlan_Dependent_New = viewPlans.Find(x => x.Name == name_Dependent);

                        if(viewPlan_Dependent_New == null)
                        {
                            ElementId elementId_Dependent_New = viewPlan_New.Duplicate(ViewDuplicateOption.AsDependent);
                            if (elementId_Dependent_New == null || elementId_Dependent_New == ElementId.InvalidElementId)
                                continue;

                            viewPlan_Dependent_New = document.GetElement(elementId_Dependent_New) as ViewPlan;
                            if (viewPlan_Dependent_New == null)
                                continue;

                            // Check name uniqueness
                            string name_Dependent_Temp = name_Dependent;
                            int count = 0;
                            while (viewPlans.Find(x => x.Name == name_Dependent_Temp) != null)
                            {
                                count++;
                                name_Dependent_Temp = string.Format("{0} {1}", name_Dependent, count);
                            }

                            viewPlan_Dependent_New.Name = name_Dependent_Temp;
                        }

                        foreach (Parameter parameter in viewPlan_Dependent.ParametersMap)
                        {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                            if (parameter.Id.IntegerValue == (int)BuiltInParameter.VIEW_NAME)
                                continue;
#else
                            if (parameter.Id.Value == (long)BuiltInParameter.VIEW_NAME)
                                continue;
#endif

                            Definition definition = parameter?.Definition;
                            if (definition == null)
                                continue;

                            Parameter parameter_New = viewPlan_Dependent_New.get_Parameter(parameter.Definition);
                            if (parameter_New == null)
                                continue;

                            CopyValue(parameter, parameter_New);
                        }

                        result.Add(viewPlan_Dependent_New);
                    }
                }
            }

            return result;
        }
    }
}