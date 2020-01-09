using Autodesk.Revit.DB;

namespace SAMAnalyticalDynamoRevit
{
    /// <summary>
    /// SAM Analytical Space
    /// </summary>
    public static class Space
    {
        /// <summary>
        /// Creates SAM Analytical Space from Revit 
        /// </summary>
        /// <param name="element">Revit SpatialElement</param>
        /// <search>
        /// FromRevit, SAM Analytical Space
        /// </search>
        public static SAM.Analytical.Space FromRevit(Revit.Elements.Element element)
        {
            return SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement as SpatialElement);
        }
    }
}
