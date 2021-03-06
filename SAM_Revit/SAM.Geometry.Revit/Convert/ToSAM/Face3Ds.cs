﻿using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Face3D> ToSAM(this Autodesk.Revit.DB.Face face)
        {
            return Spatial.Create.Face3Ds(face.ToSAM_Polygon3Ds(), EdgeOrientationMethod.Undefined);
        }

        public static List<Face3D> ToSAM_Face3Ds(this Sketch sketch, bool flip = false)
        {
            if (sketch == null)
                return null;

            CurveArrArray profile = null;
            try
            {
                profile = sketch.Profile;
            }
            catch
            {
                profile = null;
            }

            if (profile == null)
                return null;

            XYZ normal = sketch.SketchPlane?.GetPlane()?.Normal;

            List<Face3D> result = new List<Face3D>();
            foreach (CurveArray curveArray in sketch.Profile)
                result.Add(curveArray.ToSAM_Face3D(normal, flip));

            return result;
        }

        public static List<Face3D> ToSAM_Face3Ds(this Solid solid)
        {
            return solid?.ToSAM()?.Face3Ds;
        }

        public static List<Face3D> ToSAM_Face3Ds(this Mesh mesh)
        {
            List<Triangle3D> triangle3Ds = ToSAM_Triangle3Ds(mesh);
            if(triangle3Ds == null)
            {
                return null;
            }

            return triangle3Ds.ConvertAll(x => new Face3D(x));
        }

    }
}