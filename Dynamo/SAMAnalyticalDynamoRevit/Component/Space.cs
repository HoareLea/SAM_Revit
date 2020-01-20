using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System.Collections.Generic;

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

        /// <summary>
        /// Creates Space from SAM Analytical Space
        /// </summary>
        /// <param name="space">SAM Analytical Space</param>
        /// <param name="includePanels">Include panels in conversion</param>
        /// <search>
        /// ToRevit, SAM Analytical Space
        /// </search>
        public static Revit.Elements.Element ToRevit(SAM.Analytical.Space space)
        {
            Document document = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(document);

            Autodesk.Revit.DB.Element element = SAM.Analytical.Revit.Convert.ToRevit(document, space);

            TransactionManager.Instance.TransactionTaskDone();

            if (element != null)
                return ElementWrapper.ToDSType(element, true);
            else
                return null;
        }

        public static IEnumerable<SAM.Analytical.Space> FromRevitLinkInstance(Revit.Elements.Element revitLinkInstance, bool fromRooms = true)
        {
            TransactionManager.Instance.ForceCloseTransaction();

            RevitLinkInstance revitLinkInstance_Revit = revitLinkInstance.InternalElement as RevitLinkInstance;
            if (revitLinkInstance_Revit == null)
                return null;

            return SAM.Analytical.Revit.Convert.ToSAM_Spaces(revitLinkInstance_Revit, fromRooms);
        }
    }
}
