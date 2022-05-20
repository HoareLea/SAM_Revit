using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Planar.Face2D> BoundaryFace2Ds(this Space space)
        {
            if (space == null)
                return null;

            SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;

            return BoundaryFace2Ds(space, spatialElementBoundaryOptions);
        }

        public static List<Planar.Face2D> BoundaryFace2Ds(this Space space, SpatialElementBoundaryOptions spatialElementBoundaryOptions)
        {
            if(space == null || double.IsNaN(space.Area) || space.Area == 0)
                return null;
            
            if(spatialElementBoundaryOptions == null)
            {
                spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            }

            IList<IList<BoundarySegment>> boundaries = space?.GetBoundarySegments(spatialElementBoundaryOptions);
            if (boundaries == null)
                return null;

            List<Planar.Polygon2D> polygon2Ds = new List<Planar.Polygon2D>();
            foreach(IList<BoundarySegment> boundarySegments in boundaries)
            {
                List<Planar.Point2D> point2Ds = new List<Planar.Point2D>();
                foreach (BoundarySegment boundarySegment in boundarySegments)
                {
                    List<Spatial.Segment3D> segment3Ds = boundarySegment?.GetCurve()?.ToSAM_Segment3Ds();
                    if (segment3Ds == null || segment3Ds.Count == 0)
                    {
                        point2Ds = null;
                        break;
                    }

                    foreach(Spatial.Segment3D segment3D in segment3Ds)
                        point2Ds.Add(new Planar.Point2D(segment3D[0].X, segment3D[0].Y));
                }

                if (point2Ds == null || point2Ds.Count < 3)
                    continue;

                Planar.Polygon2D polygon2D = new Planar.Polygon2D(point2Ds);
                polygon2Ds.Add(polygon2D);
            }

            return Planar.Create.Face2Ds(polygon2Ds, EdgeOrientationMethod.Undefined);
        }
    }
}