using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static IEnumerable<T> Elements<T>(this Document document, Point3D point3D, double maxDistance = Core.Tolerance.MacroDistance)
        {
            if (document == null || point3D == null)
                return null;

            XYZ xyz = point3D.ToRevit();

            Outline outline = new Outline(new XYZ(xyz.X - maxDistance, xyz.Y - maxDistance, xyz.Z - maxDistance), new XYZ(xyz.X + maxDistance, xyz.Y + maxDistance, xyz.Z + maxDistance));

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new ElementFilter[] { new BoundingBoxIntersectsFilter(outline), new BoundingBoxIsInsideFilter(outline) });

            return new FilteredElementCollector(document).OfClass(typeof(T)).WherePasses(logicalOrFilter).Cast<T>();
        }

        public static IEnumerable<T> Elements<T>(this Document document, BoundingBox3D boundingBox3D)
        {
            if (document == null || boundingBox3D == null)
                return null;

            Outline outline = new Outline(boundingBox3D.Min.ToRevit(), boundingBox3D.Max.ToRevit());

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new ElementFilter[] { new BoundingBoxIntersectsFilter(outline), new BoundingBoxIsInsideFilter(outline) });

            return new FilteredElementCollector(document).OfClass(typeof(T)).WherePasses(logicalOrFilter).Cast<T>();
        }

        public static IEnumerable<Element> Elements(this Document document, BoundingBox3D boundingBox3D, params BuiltInCategory[] builtInCategories)
        {
            if (document == null || boundingBox3D == null)
                return null;

            Outline outline = new Outline(boundingBox3D.Min.ToRevit(), boundingBox3D.Max.ToRevit());

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new ElementFilter[] { new BoundingBoxIntersectsFilter(outline), new BoundingBoxIsInsideFilter(outline) });

            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document);
            if(builtInCategories != null && builtInCategories.Length != 0)
                filteredElementCollector.WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));

            return new FilteredElementCollector(document).WherePasses(logicalOrFilter);
        }
    }
}