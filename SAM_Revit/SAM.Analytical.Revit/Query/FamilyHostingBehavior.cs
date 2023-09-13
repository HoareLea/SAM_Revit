using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static HashSet<FamilyHostingBehavior> FamilyHostingBehaviors(this PanelGroup panelGroup)
        {
            if(panelGroup == Analytical.PanelGroup.Undefined)
            {
                return null;
            }

            switch(panelGroup)
            {
                case Analytical.PanelGroup.Floor:
                    return new HashSet<FamilyHostingBehavior>() { FamilyHostingBehavior.Floor, FamilyHostingBehavior.Face, FamilyHostingBehavior.None };

                case Analytical.PanelGroup.Wall:
                    return new HashSet<FamilyHostingBehavior>() { FamilyHostingBehavior.Wall, FamilyHostingBehavior.Face, FamilyHostingBehavior.None };

                case Analytical.PanelGroup.Roof:
                    return new HashSet<FamilyHostingBehavior>() { FamilyHostingBehavior.Roof, FamilyHostingBehavior.Face, FamilyHostingBehavior.None };

                case Analytical.PanelGroup.Other:
                    return new HashSet<FamilyHostingBehavior>() { FamilyHostingBehavior.Roof, FamilyHostingBehavior.Face, FamilyHostingBehavior.None, FamilyHostingBehavior.Wall, FamilyHostingBehavior.Ceiling };

            }

            return null;

            
        }
    }
}