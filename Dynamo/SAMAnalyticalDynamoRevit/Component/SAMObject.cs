using RevitServices.Transactions;
using SAM.Core.Revit;
using System.Collections.Generic;

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
        /// <search>FromRevit, SAM Analytical Panel</search>
        public static IEnumerable<object> FromRevit(Revit.Elements.Element element)
        {
            TransactionManager.Instance.ForceCloseTransaction();

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            IEnumerable<SAM.Core.SAMObject> sAMObjects = SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement, convertSettings);
            if (sAMObjects == null)
                return null;

            return sAMObjects;
        }
    }
}