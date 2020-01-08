using System.Collections.Generic;

using Autodesk.Revit.DB;
using RevitServices.Transactions;

namespace SAMAnalyticalDynamoRevit
{
    /// <summary>
    /// SAM Analytical Panel
    /// </summary>
    public static class Panel
    {
        /// <summary>
        /// Create Panel from Revit Element
        /// </summary>
        /// <param name="element">Revit Element such as Wall, Floor, Roof</param>
        /// <search>
        /// FromElement, Analytical Panel
        /// </search>
        public static List<SAM.Analytical.Panel> FromElement(Revit.Elements.Element element)
        {
            TransactionManager.Instance.ForceCloseTransaction();

            return SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement as HostObject);
        }
    }
}
