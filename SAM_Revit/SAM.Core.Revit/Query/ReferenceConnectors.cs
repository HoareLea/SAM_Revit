using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<Connector> ReferenceConnectors(this Connector connector, ConnectorType connectorType = ConnectorType.End)
        {
            if (connector == null || !connector.IsConnected || connector.Owner == null)
            {
                return null;
            }

            ConnectorSet connectorSet = connector.AllRefs;
            if (connectorSet == null || connectorSet.Size == 0)
            {
                return null;
            }

            List<Connector> result = new List<Connector>();
            foreach (Connector connector_Ref in connectorSet)
            {
                if (connector_Ref.ConnectorType == connectorType)
                {
                    if (connector_Ref.Owner == null || connector.Owner.Id != connector_Ref.Owner.Id)
                    {
                        result.Add(connector_Ref);
                    }
                }
            }

            return result;
        }
    }
}