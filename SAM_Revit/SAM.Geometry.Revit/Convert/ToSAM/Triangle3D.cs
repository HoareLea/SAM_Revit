using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Triangle3D ToSAM(this MeshTriangle meshTriangle)
        {
            return new Triangle3D(meshTriangle.get_Vertex(0).ToSAM(), meshTriangle.get_Vertex(1).ToSAM(), meshTriangle.get_Vertex(2).ToSAM());
        }
    }
}