using Autodesk.Revit.DB;

using SAM.Geometry.Spatial;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Mesh3D ToSAM(this Mesh mesh, double tolerance = Core.Tolerance.Distance)
        {
            if(mesh == null)
            {
                return null;
            }

            return Spatial.Create.Mesh3D(mesh.ToSAM_Triangle3Ds(), tolerance);
        }
    }
}