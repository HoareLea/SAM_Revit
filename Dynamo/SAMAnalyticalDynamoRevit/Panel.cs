using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="hostObject">Revit HostObject such as Wall, Floor, Roof</param>
        /// <search>
        /// Topologic, SpaceAdjacency, Analytical Panel
        /// </search>
        public static List<SAM.Analytical.Panel> FromHostObject(HostObject hostObject)
        {
            return SAM.Analytical.Revit.Convert.ToSAM(hostObject);
        }
    }
}
