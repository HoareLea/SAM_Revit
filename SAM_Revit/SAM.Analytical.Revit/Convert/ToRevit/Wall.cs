using Autodesk.Revit.DB;
using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        /// <summary>
        /// Converts Face3D to Wall
        /// </summary>
        /// <param name="face3D">SAM Geometry Face3D</param>
        /// <param name="document">Revit Document</param>
        /// <param name="wallType">Revit WallType. Default WallType will be used if null</param>
        /// <param name="level">Revit Level. Low Level for Face3D will be used if null</param>
        /// <returns>Revit Wall</returns>
        public static Autodesk.Revit.DB.Wall ToRevit_Wall(this Geometry.Spatial.Face3D face3D, Document document, Autodesk.Revit.DB.WallType wallType = null, Level level = null)
        {
            if (face3D == null || document == null)
            {
                return null;
            }

            if (face3D.GetArea() < Tolerance.MacroDistance)
            {
                return null;
            }

            Geometry.Spatial.Plane plane = face3D.GetPlane();

            face3D = face3D.SimplifyByNTS_TopologyPreservingSimplifier();
            face3D = face3D.SimplifyByAngle();

            Vector3D normal = plane.Normal;
            if (normal == null)
            {
                return null;
            }

            if (wallType == null)
            {
                ElementId elementId = document.Settings.Categories.get_Item(BuiltInCategory.OST_Walls).Id;
                if (elementId != null && elementId != ElementId.InvalidElementId)
                {
                    elementId = document.GetDefaultFamilyTypeId(elementId);
                    if (elementId != null && elementId != ElementId.InvalidElementId)
                    {
                        wallType = document.GetElement(elementId) as Autodesk.Revit.DB.WallType;
                    }
                }
            }

            if (wallType == null)
            {
                return null;
            }

            if (level == null)
            {
                double elevation = Geometry.Revit.Convert.ToRevit(face3D.GetBoundingBox().Min).Z;
                level = Core.Revit.Query.LowLevel(document, elevation);
            }

            if (level == null)
            {
                return null;
            }

            //Face3D face3D_Temp = face3D;

            //if (normal.Z.AlmostEqual(0, Tolerance.MacroDistance))
            //{
            //    normal = new Vector3D(normal.X, normal.Y, 0).Unit;
            //    plane = new Geometry.Spatial.Plane(plane.Origin, normal);

            //    face3D_Temp = plane.Project(face3D);
            //}

            XYZ xyz_Normal = Geometry.Revit.Convert.ToRevit(normal, false);

            List<CurveLoop> curveLoops = Geometry.Revit.Convert.ToRevit(face3D);
            curveLoops?.RemoveAll(x => x == null);
            if (curveLoops == null || curveLoops.Count == 0)
            {
                return null;
            }

            if (curveLoops.Count == 1)
            {
                return Autodesk.Revit.DB.Wall.Create(document, curveLoops[0].ToList(), wallType.Id, level.Id, false, xyz_Normal);
            }

            //The Wall.Create method requires the curveLoops to be in either all counter-clockwise direction or all clockwise direction.
            for (int i = 0; i < curveLoops.Count; i++)
            {
                if (curveLoops[i].IsCounterclockwise(xyz_Normal))
                {
                    curveLoops[i].Flip();
                }
            }

            List<Curve> curves = new List<Curve>();


            //According to https://forums.autodesk.com/t5/revit-api-forum/wall-create-by-profile/td-p/8961085
            //CurveLoops have to be organised in correct order. Hypothesis :
            //If the curveLoop contains 1 (or more) vertical Line(s), the vertical line should be the first to add to the List.
            foreach (CurveLoop curveLoop in curveLoops)
            {
                List<Curve> curves_Temp = curveLoop?.ToList();
                if (curveLoop == null)
                {
                    continue;
                }

                List<Curve> curves_Postponed = new List<Curve>();

                int startindex = 0;
                while (startindex < curves_Temp.Count)
                {
                    Curve curve = curves_Temp[startindex];
                    if (!(curve is Line))
                    {
                        curves_Postponed.Add(curve);
                        startindex++;
                        continue;
                    }
                    XYZ dir = curve.GetEndPoint(1).Subtract(curve.GetEndPoint(0)).Normalize();
                    if (!dir.IsAlmostEqualTo(XYZ.BasisZ) && !dir.IsAlmostEqualTo(XYZ.BasisZ.Negate()))
                    {
                        curves_Postponed.Add(curve);
                        startindex++;
                    }
                    else break;
                }

                for (int i = startindex; i < curves_Temp.Count; i++)
                {
                    Curve curve = curves_Temp[i];
                    curves.Add(curve);
                }

                if (curves_Postponed.Count > 0)
                {
                    curves.AddRange(curves_Postponed);
                }
            }

            if (curves == null || curves.Count == 0)
            {
                return null;
            }

            return Autodesk.Revit.DB.Wall.Create(document, curves, wallType.Id, level.Id, false, xyz_Normal);

        }
    }
}