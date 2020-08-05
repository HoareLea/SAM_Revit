using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using SAM.Core.Revit;
using System.Collections.Generic;

namespace SAMAnalyticalDynamoRevit
{
    /// <summary>
    /// SAM Analytical Panel
    /// </summary>
    public static class Panel
    {
        /// <summary>
        /// Creates Panel from Revit Element
        /// </summary>
        /// <param name="element">Revit Element such as Wall, Floor, Roof</param>
        /// <search>FromRevit, SAM Analytical Panel</search>
        public static List<SAM.Analytical.Panel> FromRevit(Revit.Elements.Element element)
        {
            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            return SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement as HostObject, convertSettings);
        }

        public static List<SAM.Analytical.Panel> FromRevitLinkInstance(Revit.Elements.Element revitLinkInstance)
        {
            RevitLinkInstance revitLinkInstance_Revit = revitLinkInstance.InternalElement as RevitLinkInstance;
            if (revitLinkInstance_Revit == null)
                return null;

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            return SAM.Analytical.Revit.Convert.ToSAM_Panels(revitLinkInstance_Revit, convertSettings);
        }

        /// <summary>
        /// Creates HostObject from SAM Analytical Panel
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <search>ToRevit, SAM Analytical Panel</search>
        public static Revit.Elements.Element ToRevit(SAM.Analytical.Panel panel, ConvertSettings convertSettings)
        {
            Document document = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(document);

            HostObject hostObject = SAM.Analytical.Revit.Convert.ToRevit(panel, document, convertSettings);

            TransactionManager.Instance.TransactionTaskDone();

            if (hostObject != null)
                return ElementWrapper.ToDSType(hostObject, true);
            else
                return null;
        }

        /// <summary>
        /// Creates HostObject from SAM Analytical Panel
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <search>ToRevit, SAM Analytical Panel</search>
        public static Revit.Elements.Element ToRevit(SAM.Analytical.Panel panel)
        {
            Document document = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(document);

            ConvertSettings convertSettings = Query.ConvertSettings();

            HostObject hostObject = SAM.Analytical.Revit.Convert.ToRevit(panel, document, convertSettings);

            TransactionManager.Instance.TransactionTaskDone();

            if (hostObject != null)
                return ElementWrapper.ToDSType(hostObject, true);
            else
                return null;
        }
    }
}