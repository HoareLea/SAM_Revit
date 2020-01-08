using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Topologic, SpaceAdjacency, Analytical Panel
        /// </search>
        public static SAM.Analytical.Space FromElement(Revit.Elements.Element element)
        {
            return SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement as SpatialElement);
        }
    }
}
