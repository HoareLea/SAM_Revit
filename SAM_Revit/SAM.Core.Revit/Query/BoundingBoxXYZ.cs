using Autodesk.Revit.DB;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static BoundingBoxXYZ BoundingBoxXYZ(this Element scopeBox)
        {
            Document document = scopeBox.Document;
            GeometryElement geometryElement = scopeBox.get_Geometry(document.Application.Create.NewGeometryOptions());

            if (geometryElement ==null || geometryElement.Count() != 12)
                return null;

            XYZ origin = null;
            XYZ vx = null;
            XYZ vy = null;
            XYZ vz = null;

            foreach (GeometryObject geometryObject in geometryElement)
            {
                Line line = geometryObject as Line;
                if (line == null)
                    continue;

                XYZ p = line.GetEndPoint(0);
                XYZ q = line.GetEndPoint(1);
                XYZ v = q - p;

                if (null == origin)
                {
                    origin = p;
                    vx = v;
                }
                else if (p.IsAlmostEqualTo(origin) || q.IsAlmostEqualTo(origin))
                {
                    if (q.IsAlmostEqualTo(origin))
                    {
                        v = v.Negate();
                    }
                    if (vy == null)
                    {
                        vy = v;
                    }
                    else
                    {
                        vz = v;

                        if (0 >= vx.CrossProduct(vy).DotProduct(vz))
                        {
                            XYZ tmp = vz;
                            vz = vy;
                            vy = tmp;
                        }
                        break;
                    }
                }
            }

            // Set up the transform

            Transform t = Transform.Identity;
            t.Origin = origin;
            t.BasisX = vx.Normalize();
            t.BasisY = vy.Normalize();
            t.BasisZ = vz.Normalize();


            // Set up the bounding box

            BoundingBoxXYZ result = new BoundingBoxXYZ();
            result.Transform = t;
            result.Min = XYZ.Zero;
            result.Max = new XYZ(vx.GetLength(), vy.GetLength(), vz.GetLength());

            return result;
        }
    }
}