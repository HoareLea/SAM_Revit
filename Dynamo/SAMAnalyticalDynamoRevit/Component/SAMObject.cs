using System.Collections.Generic;
using System.Linq;
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
        public static IEnumerable<object> FromRevit(Revit.Elements.Element element)
        {
            TransactionManager.Instance.ForceCloseTransaction();

            IEnumerable<SAM.Core.SAMObject> sAMObjects = SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement);
            if (sAMObjects == null)
                return null;

            return sAMObjects;
        }
    }
}
