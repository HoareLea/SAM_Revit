using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static bool IsValidFamilyHostingBehavior(this Family family, PanelGroup panelGroup)
        {
            if(family == null || panelGroup == Analytical.PanelGroup.Undefined)
            {
                return false;
            }

            HashSet<FamilyHostingBehavior> familyHostingBehaviors = FamilyHostingBehaviors(panelGroup);
            if (familyHostingBehaviors != null && familyHostingBehaviors.Count != 0)
            {
                return false;
            }

            Parameter parameter = family.get_Parameter(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR);
            if (parameter != null && parameter.HasValue)
            {
                FamilyHostingBehavior familyHostingBehavior = (FamilyHostingBehavior)parameter.AsInteger();
                return familyHostingBehaviors.Contains(familyHostingBehavior);
            }

            return false;
        }

        public static bool IsValidFamilyHostingBehavior(this FamilySymbol familySymbol, PanelGroup panelGroup)
        {
            return IsValidFamilyHostingBehavior(familySymbol?.Family, panelGroup);
        }
    }
}