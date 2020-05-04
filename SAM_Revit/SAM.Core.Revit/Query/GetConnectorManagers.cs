using Autodesk.Revit.DB;
using System.Collections.Generic;

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
                    if (connector_Ref.Owner.Id != element.Id && !(connector_Ref.Owner is MEPSystem) && !(connector_Ref.Owner is InsulationLiningBase))
                        if (result.Find(x => x.Owner.Id == connector_Ref.ConnectorManager.Owner.Id) == null)
                            result.Add(connector_Ref.ConnectorManager);
                }
            }

            return result;
        }
    }
}