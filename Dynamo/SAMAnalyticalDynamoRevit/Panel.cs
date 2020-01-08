using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace SAMAnalyticalDynamoRevit
{
    /// <summary>
    /// SAM Analytical Panel
    /// </summary>
    public static class Panel
    {
        /// <summary>
        /// Extract Space Adjacency information for Panels
        /// </summary>
        /// <param name="element">Revit Element such as Wall, Floor, Roof</param>
        /// <search>
        /// Topologic, SpaceAdjacency, Analytical Panel
        /// </search>
        public static List<SAM.Analytical.Panel> FromElement(Revit.Elements.Element element)
        {
            return SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement as HostObject);
        }
    }
}
