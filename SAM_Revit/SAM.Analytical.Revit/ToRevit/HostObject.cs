using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObject ToRevit(this Document document, Panel panel)
        {
            Geometry.Spatial.Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            HostObjAttributes aHostObjAttributes = document.ToRevit(panel.Construction, panel.PanelType);

            HostObject result = null;
            if (aHostObjAttributes is WallType)
            {
                List<Curve> curveList = new List<Curve>();
                foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in face3D.GetEdges())
                {
                    if (!(closedPlanar3D is Geometry.Spatial.ICurvable3D))
                        continue;

                    curveList.AddRange(Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves()).ConvertAll(x => x.ToRevit_Line()));
                }

                if (curveList == null || curveList.Count == 0)
                    return null;

                double lowElevation = panel.LowElevation();

                Level level = document.LowLevel(lowElevation);
                if (level == null)
                    return null;

                //Flipping recognition

                Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.Base;

                //Get Normal from Panel
                Geometry.Spatial.Vector3D vector3D_1 =  plane.Project(panel.PlanarBoundary3D.GetNormal());
                vector3D_1 = vector3D_1.Unit;

                XYZ vectorRevit = vector3D_1.ToRevit().Normalize();

                result = Wall.Create(document, curveList, aHostObjAttributes.Id, level.Id, false, vectorRevit);

                Parameter parameter = null;

                parameter = result.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                parameter = result.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (parameter != null)
                {
                    double height = UnitUtils.ConvertToInternalUnits((panel.HightElevation() - lowElevation), DisplayUnitType.DUT_METERS);
                    parameter.Set(height);
                }

                double levelElevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
                if (System.Math.Abs(lowElevation - levelElevation) > SAM.Geometry.Tolerance.MacroDistance)
                {
                    parameter = result.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                    if (parameter != null)
                        parameter.Set(UnitUtils.ConvertToInternalUnits(lowElevation - levelElevation, DisplayUnitType.DUT_METERS));
                }

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null)
                    apertures.ForEach(x => Convert.ToRevit(document, x, result));

                return result;
            }
            else if (aHostObjAttributes is FloorType)
            {
                Geometry.Spatial.IClosedPlanar3D closedPlanar3D_External = face3D.GetExternalEdge();
                if (!(closedPlanar3D_External is Geometry.Spatial.ICurvable3D))
                    return null;

                CurveArray curveArray = new CurveArray();
                foreach (Line line in Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D_External).GetCurves()).ConvertAll(x => x.ToRevit_Line()))
                    curveArray.Append(line);

                Level level = document.HighLevel(panel.LowElevation());

                result = document.Create.NewFloor(curveArray, aHostObjAttributes as FloorType, level, false);
                result.ChangeTypeId(aHostObjAttributes.Id);

                //TODO: solve issue with transaction and openings
                //https://thebuildingcoder.typepad.com/blog/2013/07/create-a-floor-with-an-opening-or-complex-boundary.html
                //List<Geometry.Spatial.IClosedPlanar3D> closedPlanar3Ds_Internal = face3D.GetInternalEdges();
                //if(closedPlanar3Ds_Internal != null && closedPlanar3Ds_Internal.Count > 0)
                //{
                //    foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D_Internal in face3D.GetInternalEdges())
                //    {
                //        curveArray = new CurveArray();
                //        foreach (Line line in Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D_Internal).GetCurves()).ConvertAll(x => x.ToRevit_Line()))
                //            curveArray.Append(line);

                //        Opening opening = document.Create.NewOpening(result, curveArray, true);
                //    }
                //}

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null)
                    apertures.ForEach(x => Convert.ToRevit(document, x, result));

                return result;

            }
            else if(aHostObjAttributes is RoofType)
            {
                CurveArray curveArray = new CurveArray();
                foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in face3D.GetEdges())
                {
                    if (!(closedPlanar3D is Geometry.Spatial.ICurvable3D))
                        continue;

                    List<Geometry.Spatial.ICurve3D> curve3Ds = Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves());
                    if (curve3Ds == null || curve3Ds.Count == 0)
                        continue;

                    foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                        curveArray.Append(curve3D.ToRevit_Line());

                }

                Level level = document.HighLevel(panel.LowElevation());
                double levelElevation = level.Elevation;

                ModelCurveArray modelCurveArray = new ModelCurveArray();
                RoofBase roofBase = document.Create.NewFootPrintRoof(curveArray, level, aHostObjAttributes as RoofType, out modelCurveArray);

                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                SlabShapeEditor slabShapeEditor = roofBase.SlabShapeEditor;
                slabShapeEditor.ResetSlabShape();

                foreach (Curve curve in curveArray)
                {
                    XYZ xYZ = curve.GetEndPoint(0);
                    if (Math.Abs(xYZ.Z - levelElevation) > Geometry.Tolerance.MicroDistance)
                        slabShapeEditor.DrawPoint(xYZ);
                }

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null)
                    apertures.ForEach(x => Convert.ToRevit(document, x, result));

                return roofBase;
            }

            return null;
        }
    }
}
