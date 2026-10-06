// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
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

            tolerance_Temp = UnitUtils.ConvertToInternalUnits(tolerance_Temp, UnitTypeId.Meters);

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