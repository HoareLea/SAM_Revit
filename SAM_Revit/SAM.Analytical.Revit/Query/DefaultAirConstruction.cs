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

            return constructionLibrary.GetConstructions(panelGroup)?.FirstOrDefault();
        }
    }
}