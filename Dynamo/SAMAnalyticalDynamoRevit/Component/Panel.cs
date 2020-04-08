using System.Collections.Generic;

using Autodesk.Revit.DB;

using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

using Autodesk.DesignScript.Runtime;

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
        /// <search>
        /// FromRevit, SAM Analytical Panel
        /// </search>
        public static List<SAM.Analytical.Panel> FromRevit(Revit.Elements.Element element)
        {
            return SAM.Analytical.Revit.Convert.ToSAM(element.InternalElement as HostObject);
        }

        public static List<SAM.Analytical.Panel> FromRevitLinkInstance(Revit.Elements.Element revitLinkInstance)
        {
            RevitLinkInstance revitLinkInstance_Revit = revitLinkInstance.InternalElement as RevitLinkInstance;
            if (revitLinkInstance_Revit == null)
                return null;

            return SAM.Analytical.Revit.Convert.ToSAM_Panels(revitLinkInstance_Revit);
        }

        /// <summary>
        /// Creates HostObject from SAM Analytical Panel
        /// </summary>
        /// <param name="panel">SAM Analytical Panel</param>
        /// <search>
        /// ToRevit, SAM Analytical Panel
        /// </search>
        public static Revit.Elements.Element ToRevit(SAM.Analytical.Panel panel, [DefaultArgument("SAMAnalyticalDynamoRevit.Panel.GetNull()")] SAM.Core.Revit.ConvertSettings convertSettings)
        {
            Document document = DocumentManager.Instance.CurrentDBDocument;
            
            TransactionManager.Instance.EnsureInTransaction(document);

            if (convertSettings == null)
                convertSettings = SAM.Core.Revit.Query.ConvertSettings();

            HostObject hostObject = SAM.Analytical.Revit.Convert.ToRevit(document, panel, convertSettings);

            TransactionManager.Instance.TransactionTaskDone();

            if (hostObject != null)
                return ElementWrapper.ToDSType(hostObject, true);
            else
                return null;
        }

        [IsVisibleInDynamoLibrary(false)]
        public  static object GetNull()
        {
            return null;
        }
    }
}
