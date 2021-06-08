using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static ConnectorProfileType ConnectorProfileType(this MEPCurve mEPCurve)
        {
            if (mEPCurve == null)
            {
                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }

            ConnectorSet connectorSet = mEPCurve?.ConnectorManager?.Connectors;
            if(connectorSet == null)
            {
                return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
            }

            foreach (Connector connector in connectorSet)
            {
                ConnectorProfileType result = connector.Shape;
                if (result != Autodesk.Revit.DB.ConnectorProfileType.Invalid)
                    return result;
            }

            return Autodesk.Revit.DB.ConnectorProfileType.Invalid;
        }
    }
}