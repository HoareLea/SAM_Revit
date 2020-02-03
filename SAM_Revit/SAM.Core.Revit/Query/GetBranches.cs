using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<List<ConnectorManager>> GetBranches(ConnectorManager connectorManager)
        {
            return GetBranches(connectorManager, null, null);
        }

        private static List<List<ConnectorManager>> GetBranches(this ConnectorManager connectorManager, ConnectorManager previousConnectorManager = null, List<ElementId> elementIds = null)
        {
            if (connectorManager == null)
                return null;

            if (elementIds == null)
                elementIds = new List<ElementId>();

            List<List<ConnectorManager>> result = new List<List<ConnectorManager>>();
            int index = elementIds.IndexOf(connectorManager.Owner.Id);
            if (index != -1)
            {
                result.Add(new List<ConnectorManager> { connectorManager });
                elementIds.RemoveRange(index + 1, elementIds.Count - index - 1);
                return result;
            }

            elementIds.Add(connectorManager.Owner.Id);

            List<ConnectorManager> connectorManagerList = GetConnectorManagers(connectorManager);
            if (connectorManagerList == null || connectorManagerList.Count == 0)
            {
                result.Add(new List<ConnectorManager> { connectorManager });
                return result;
            }

            if (previousConnectorManager != null)
                connectorManagerList.RemoveAll(x => x.Owner.Id == previousConnectorManager.Owner.Id);

            if (connectorManagerList == null || connectorManagerList.Count == 0)
            {
                result.Add(new List<ConnectorManager> { connectorManager });
                return result;
            }

            foreach (ConnectorManager connectorManager_Temp in connectorManagerList)
            {
                List<List<ConnectorManager>> connectorManagers = GetBranches(connectorManager_Temp, connectorManager, elementIds);
                if (connectorManagers == null || connectorManagers.Count == 0)
                {
                    result.Add(new List<ConnectorManager> { connectorManager });
                    continue;
                }
                foreach (List<ConnectorManager> aConnectorManagerList_Temp1 in connectorManagers)
                {
                    List<ConnectorManager> aConnectorManagerList_Temp2 = new List<ConnectorManager>();
                    aConnectorManagerList_Temp2.Add(connectorManager);

                    aConnectorManagerList_Temp2.AddRange(aConnectorManagerList_Temp1);
                    result.Add(aConnectorManagerList_Temp2);
                }
            }

            return result;
        }

    }
}
