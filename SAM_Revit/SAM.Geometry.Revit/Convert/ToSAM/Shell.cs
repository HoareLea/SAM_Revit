﻿using Autodesk.Revit.DB;
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

        public static Shell ToSAM(this Solid solid)
        {
            if(solid == null)
            {
                return null;
            }

            FaceArray faceArray = solid.Faces;
            if (faceArray == null)
            {
                return null;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (Autodesk.Revit.DB.Face face in faceArray)
            {
                List<Face3D> face3Ds_Temp = null;
                if (face is PlanarFace)
                {
                    face3Ds_Temp = face.ToSAM();
                }
                else
                {
                    face3Ds_Temp = face.Triangulate()?.ToSAM_Face3Ds();
                }

                if (face3Ds_Temp != null && face3Ds_Temp.Count != 0)
                    face3Ds.AddRange(face3Ds_Temp);

            }

            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            return new Shell(face3Ds);
        }
    }
}