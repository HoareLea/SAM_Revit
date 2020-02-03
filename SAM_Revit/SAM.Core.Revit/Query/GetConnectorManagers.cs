using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<ConnectorManager> GetConnectorManagers(this ConnectorManager connectorManager)
        {
            if (connectorManager == null)
                return null;

            Element element = connectorManager.Owner;
            List<ConnectorManager> result = new List<ConnectorManager>();
            foreach (Connector connector in connectorManager.Connectors)
            {
                foreach (Connector connector_Ref in connector.AllRefs)
                {
                    if (connector_Ref.Owner.Id != element.Id && !(connector_Ref.Owner is MEPSystem))
                        result.Add(connector_Ref.ConnectorManager);
                }
            }

            return result;
        }
    }
}
