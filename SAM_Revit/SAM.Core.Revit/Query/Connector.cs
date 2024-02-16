using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Connector Connector(this ConnectorManager connectorManager, XYZ xYZ, ConnectorType connectorType = ConnectorType.End, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (connectorManager == null || xYZ == null)
            {
                return null;
            }

            ConnectorSet connectorSet = connectorManager.Connectors;
            if(connectorSet == null)
            {
                return null;
            }

            double tolerance_Temp = tolerance;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
            tolerance_Temp = UnitUtils.ConvertToInternalUnits(tolerance_Temp, DisplayUnitType.DUT_METERS);
#else
            tolerance_Temp = UnitUtils.ConvertToInternalUnits(tolerance_Temp, UnitTypeId.Meters);
#endif

            foreach (Connector connector in connectorSet)
            {
                if (connector.ConnectorType == connectorType && connector.Origin.DistanceTo(xYZ) <= tolerance_Temp)
                {
                    return connector;
                }
            }

            return null;
        }
    }
}