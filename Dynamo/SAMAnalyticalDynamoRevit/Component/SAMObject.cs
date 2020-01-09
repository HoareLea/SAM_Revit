using System.Collections.Generic;

using Autodesk.Revit.DB;

using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace SAMAnalyticalDynamoRevit
{
    /// <summary>
    /// SAM Object
    /// </summary>
    public static class SAMObject
    {
        /// <summary>
        /// Creates Panel from Revit Element
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <search>
        /// FromRevit, SAM Analytical Panel
        /// </search>
        public static IEnumerable<SAM.Core.SAMObject> FromRevit(Revit.Elements.Element element)
        {
            return SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement);
        }

        ///// <summary>
        ///// Creates HostObject from SAM Analytical Panel
        ///// </summary>
        ///// <param name="sAMObject">SAM Object</param>
        ///// <search>
        ///// ToRevit, SAM Analytical Panel
        ///// </search>
        //public static Revit.Elements.Element ToRevit(SAM.Core.SAMObject sAMObject)
        //{
        //}
    }
}
