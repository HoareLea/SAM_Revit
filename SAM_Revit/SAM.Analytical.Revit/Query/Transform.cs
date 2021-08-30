using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static PlanarBoundary3D Transform(this Transform transform, PlanarBoundary3D planarBoundary3D)
        {
            if (transform == null || planarBoundary3D == null)
                return null;

            if (transform.IsIdentity)
                return new PlanarBoundary3D(planarBoundary3D);

            Geometry.Spatial.Face3D face3D = planarBoundary3D.GetFace3D();
            if(face3D == null || !Geometry.Spatial.Query.IsValid(face3D))
            {
                return null;
            }

            return new PlanarBoundary3D(Geometry.Revit.Query.Transform(transform, face3D));
        }
        
        public static Aperture Transform(this Transform transform, Aperture aperture)
        {
            if (transform == null || aperture == null)
                return null;

            if (transform.IsIdentity)
                return new Aperture(aperture);

            return new Aperture(aperture, Transform(transform, aperture.PlanarBoundary3D));
        }
        
        public static Panel Transform(this Transform transform, Panel panel)
        {
            if (transform == null || panel == null)
                return null;

            if (transform.IsIdentity)
                return Analytical.Create.Panel(panel);

            List<Aperture> apertures = panel.Apertures;
            if(apertures != null)
            {
                for (int i = 0; i < apertures.Count; i++)
                    apertures[i] = Transform(transform, apertures[i]);
            }

            Panel result = Analytical.Create.Panel(panel.Guid, panel, Transform(transform, panel.PlanarBoundary3D));
            if(result == null)
            {
                return result;
            }

            result.RemoveApertures();

            if (apertures != null)
                apertures.ForEach(x => result.AddAperture(x));

            return result;
        }

        public static Space Transform(this Transform transform, Space space)
        {
            if (transform == null || space == null)
                return null;

            if (transform.IsIdentity)
                return new Space(space);

            return new Space(space, space.Name, Geometry.Revit.Query.Transform(transform, space.Location));
        }

        public static Core.SAMObject Transform(this Transform transform, Core.SAMObject sAMObject)
        {
            if (transform == null || sAMObject == null)
                return null;

            if (transform.IsIdentity)
                return Core.Query.Clone(sAMObject);

            if (sAMObject is Panel || sAMObject is Aperture || sAMObject is Space)
                return Transform(transform, sAMObject as dynamic);

            return Core.Query.Clone(sAMObject);
        }
    }
}