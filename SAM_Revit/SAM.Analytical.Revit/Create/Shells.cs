using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Create
    {
        public static List<Shell> Shells(this Document document, IEnumerable<Autodesk.Revit.DB.Mechanical.Space> spaces = null, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (document == null)
                return null;

            List<Autodesk.Revit.DB.Mechanical.Space> spaces_Temp = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
            if(spaces != null)
            {
                List<Autodesk.Revit.DB.Mechanical.Space> spaces_New = new List<Autodesk.Revit.DB.Mechanical.Space>();
                foreach (Autodesk.Revit.DB.Mechanical.Space space in spaces)
                {
                    int index = spaces_Temp.FindIndex(x => x.Id.IntegerValue == space.Id.IntegerValue);
                    if (index != -1)
                        spaces_New.Add(spaces_Temp[index]);
                }
                spaces_Temp = spaces_New;
            }

            if (spaces_Temp == null || spaces_Temp.Count == 0)
                return null;

            List<Wall> walls = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>().ToList();
            if (walls == null || walls.Count == 0)
                return null;

            List<Panel> panels = new List<Panel>();
            foreach(Wall wall in walls)
            {
                List<Panel> panels_Wall = wall.ToSAM(new Core.Revit.ConvertSettings(true, false, false));
                if (panels_Wall == null || panels_Wall.Count == 0)
                    continue;

                panels.AddRange(panels_Wall);
            }

            List<Shell> shells = new List<Shell>();
            foreach (Autodesk.Revit.DB.Mechanical.Space space in spaces_Temp)
            {
                XYZ xyz = (space.Location as LocationPoint)?.Point;
                if (xyz == null)
                    continue;

                BoundingBoxXYZ boundingBoxXYZ = space.get_BoundingBox(null);
                if (boundingBoxXYZ == null || boundingBoxXYZ.Min.Z == boundingBoxXYZ.Max.Z)
                    continue;

                double elevation_Bottom = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Min.Z, DisplayUnitType.DUT_METERS) ;
                double elevation_Top = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Max.Z, DisplayUnitType.DUT_METERS);

                Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Bottom + offset)) as Geometry.Spatial.Plane;

                Point3D point3D = Geometry.Revit.Convert.ToSAM(xyz);
                if (point3D == null)
                    continue;

                Geometry.Planar.Point2D point2D = plane.Convert(point3D);

                List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>();
                foreach (Panel panel in panels)
                {
                    Face3D face3D = panel.GetFace3D();
                    if (face3D == null)
                        continue;

                    face3D = new Face3D(face3D.GetExternalEdge3D());

                    PlanarIntersectionResult planarIntersectionResult = plane.Intersection(face3D);
                    if (planarIntersectionResult == null)
                        continue;

                    List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Geometry.Planar.ISegmentable2D>();
                    if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                        continue;

                    foreach (Geometry.Planar.ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                }

                if (segment2Ds == null || segment2Ds.Count == 0)
                    continue;

                segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance);

                segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, tolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                polygon2Ds = polygon2Ds.FindAll(x => x.Inside(point2D));
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Geometry.Planar.Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                Geometry.Spatial.Plane plane_Bottom = Geometry.Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Bottom)) as Geometry.Spatial.Plane;
                Geometry.Spatial.Plane plane_Top = Geometry.Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Top)) as Geometry.Spatial.Plane;

                List<Face3D> face3Ds = new List<Face3D>();
                face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane_Bottom, x)));
                face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane_Top, x)));

                List<Shell> shells_Temp = Geometry.Spatial.Create.Shells(face3Ds, tolerance);
                if (shells_Temp == null || shells_Temp.Count == 0)
                    continue;

                shells.AddRange(shells_Temp);

            }

            return shells;
        }
    }
}