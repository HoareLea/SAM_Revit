using Autodesk.Revit.DB;
using System.Collections.Generic;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static bool TryGetAdaptiveComponentPoint3Ds(this FamilyInstance familyInstance, out List<Point3D> point3Ds, out List<Point3D> point3Ds_Placement, out List<Point3D> point3Ds_ShapeHandle)
        {
            point3Ds = null;
            point3Ds_Placement = null;
            point3Ds_ShapeHandle = null;

            if (familyInstance == null)
            {
                return false;
            }

            if(!AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance(familyInstance))
            {
                return false;
            }

            Document document = familyInstance.Document;

            IList<ElementId> elementIds = null;

            elementIds = AdaptiveComponentInstanceUtils.GetInstancePointElementRefIds(familyInstance);
            if (elementIds != null)
            {
                point3Ds = new List<Point3D>();
                foreach (ElementId elementId in elementIds)
                {
                    ReferencePoint referencePoint = document.GetElement(elementId) as ReferencePoint;
                    if (referencePoint == null || referencePoint.Position == null)
                    {
                        continue;
                    }

                    point3Ds.Add(referencePoint.Position.ToSAM());
                }
            }

            elementIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(familyInstance);
            if(elementIds != null)
            {
                point3Ds_Placement = new List<Point3D>();
                foreach (ElementId elementId in elementIds)
                {
                    ReferencePoint referencePoint = document.GetElement(elementId) as ReferencePoint;
                    if(referencePoint == null || referencePoint.Position == null)
                    {
                        continue;
                    }

                    point3Ds_Placement.Add(referencePoint.Position.ToSAM());
                }
            }

            elementIds = AdaptiveComponentInstanceUtils.GetInstanceShapeHandlePointElementRefIds(familyInstance);
            if (elementIds != null)
            {
                point3Ds_ShapeHandle = new List<Point3D>();
                foreach (ElementId elementId in elementIds)
                {
                    ReferencePoint referencePoint = document.GetElement(elementId) as ReferencePoint;
                    if (referencePoint == null || referencePoint.Position == null)
                    {
                        continue;
                    }

                    point3Ds_ShapeHandle.Add(referencePoint.Position.ToSAM());
                }
            }

            return true;
        }
    }
}