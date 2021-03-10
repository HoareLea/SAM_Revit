using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<ViewPlan> DuplicateViewPlan(this ViewPlan viewPlan, IEnumerable<Level> levels = null)
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


            List<ViewPlan> result = new List<ViewPlan>();
            foreach(Level level in levels_Temp)
            {
                if (level.Id == viewPlan.GenLevel.Id)
                    continue;
                
                ViewPlan viewPlan_New = ViewPlan.Create(document, viewPlan.GetTypeId(), level.Id);
                foreach(Parameter parameter in viewPlan.ParametersMap)
                {
                    Definition definition = parameter?.Definition;
                    if (definition == null)
                        continue;

                    Parameter parameter_New = viewPlan_New.get_Parameter(parameter.Definition);
                    if (parameter_New == null)
                        continue;

                    CopyValue(parameter, parameter_New);
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

                        ElementId elementId_Dependent_New = viewPlan_New.Duplicate(ViewDuplicateOption.AsDependent);
                        if (elementId_Dependent_New == null || elementId_Dependent_New == ElementId.InvalidElementId)
                            continue;

                        ViewPlan viewPlan_Dependent_New = document.GetElement(elementId_Dependent_New) as ViewPlan;
                        if (viewPlan_Dependent_New == null)
                            continue;

                        foreach (Parameter parameter in viewPlan_Dependent.ParametersMap)
                        {
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