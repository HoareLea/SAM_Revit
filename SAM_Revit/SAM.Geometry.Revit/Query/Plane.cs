using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Spatial.Plane Plane(this MEPCurve mEPCurve)
        {
            if (mEPCurve == null)
            {
                return null;
            }

            ConnectorSet connectorSet = mEPCurve?.ConnectorManager?.Connectors;
            if (connectorSet == null)
            {
                return null;
            }

            foreach (Connector connector in connectorSet)
            {
                Transform transform = connector.CoordinateSystem;
                if(transform == null)
                {
                    continue;
                }

                return new Spatial.Plane(transform.Origin.ToSAM(), transform.BasisX.ToSAM_Vector3D(true), transform.BasisY.ToSAM_Vector3D(true));
            }

            return null;
        }

        public static Spatial.Plane Plane(this View view)
        {
            if(view == null)
            {
                return null;
            }

            Spatial.Vector3D right = view.RightDirection.ToSAM_Vector3D(false);

            Spatial.Vector3D up = view.UpDirection.ToSAM_Vector3D(false);

            Spatial.Point3D origin = view.Origin.ToSAM();

            return new Spatial.Plane(origin, right, up);
        }
    }
}