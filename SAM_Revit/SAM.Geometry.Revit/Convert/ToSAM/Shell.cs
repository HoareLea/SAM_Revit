using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Shell ToSAM(this IEnumerable<Polyloop> polyloops)
        {
            if (polyloops == null || polyloops.Count() == 0)
                return null;

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(Polyloop polyloop in polyloops)
            {
                Polygon3D polygon3D = polyloop?.ToSAM();
                if (polygon3D == null)
                    continue;

                Face3D face3D = new Face3D(polygon3D);
                if (face3D != null)
                    face3Ds.Add(face3D);
            }

            if (face3Ds.Count == 0)
                return null;

            return new Shell(face3Ds);

        }
    }
}