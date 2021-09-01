using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Dictionary<ElementId, double> MaterialsAreas(this SpatialElement spatialElement, double minArea = 0, SpatialElementGeometryCalculator spatialElementGeometryCalculator = null)
        {
            if(spatialElement == null)
            {
                return null;
            }

            if (spatialElementGeometryCalculator == null)
            {
                spatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document);
            }

            SpatialElementGeometryResults spatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);
            if(spatialElementGeometryResults == null)
            {
                return null;
            }

            Solid solid = spatialElementGeometryResults.GetGeometry();
            if(solid == null)
            {
                return null;
            }

            Dictionary<ElementId, double> result = new Dictionary<ElementId, double>();
            foreach(Face face in solid.Faces)
            {
                if(face.Area < minArea)
                {
                    continue;
                }

                foreach(SpatialElementBoundarySubface spatialElementBoundarySubface in spatialElementGeometryResults.GetBoundaryFaceInfo(face))
                {
                    Face face_SpatialElement = spatialElementBoundarySubface.GetSpatialElementFace();
                    if (face_SpatialElement == null)
                        continue;

                    if (face_SpatialElement.Area < minArea)
                    {
                        continue;
                    }

                    Face face_BoundingElement = spatialElementBoundarySubface.GetBoundingElementFace();
                    if (face_BoundingElement == null)
                    {
                        continue;
                    }

                    ElementId elementId = face_BoundingElement.MaterialElementId;
                    if (elementId != null && elementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                    {
                        if (result.ContainsKey(elementId))
                        {
                            result[elementId] += face_SpatialElement.Area;
                        }
                        else
                        {
                            result[elementId] = face_SpatialElement.Area;
                        }
                    }
                }
            }

            return result;
        }
    }
}