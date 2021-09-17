using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static Construction DefaultAirConstruction(this PanelGroup panelGroup)
        {
            ConstructionLibrary constructionLibrary = DefaultAirConstructionLibrary();
            if (constructionLibrary == null)
                return null;

            Construction result = constructionLibrary.GetConstructions(panelGroup)?.FirstOrDefault();
            if(result == null || panelGroup == PanelGroup.Roof)
            {
                result = constructionLibrary.GetConstructions(PanelGroup.Floor)?.FirstOrDefault();
            }

            return result;
        }
    }
}