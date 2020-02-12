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
            Geometry.Spatial.ICurvable3D curvable3D = panel.GetFace3D().GetExternalEdge() as Geometry.Spatial.ICurvable3D;
            if (curvable3D == null)
                return null;

            HostObjAttributes aHostObjAttributes = document.ToRevit(panel.Construction, panel.PanelType);

            HostObject result = null;
            if (aHostObjAttributes is WallType)
            {
                List<Curve> curveList = new List<Curve>();
                foreach (Geometry.Spatial.ICurve3D curve3D in curvable3D.GetCurves())
                    curveList.Add(curve3D.ToRevit_Line());

                double lowElevation = panel.LowElevation();

                Level level = document.LowLevel(lowElevation);
                if (level == null)
                    return null;
                
                //result = Wall.Create(document, curveList, aHostObjAttributes.Id, level.Id, false);

                //Flipping recognition


                Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.Base;

                //Get Normal from Panel
                Geometry.Spatial.Vector3D vector3D_1 =  plane.Project(panel.PlanarBoundary3D.GetNormal());
                vector3D_1 = vector3D_1.Unit;

                ////Get vector from Wall Location Line
                //Geometry.Spatial.Vector3D vector3D_2 = new Geometry.Spatial.Vector3D((result.Location as LocationCurve).Curve.GetEndPoint(0).ToSAM(), (result.Location as LocationCurve).Curve.GetEndPoint(1).ToSAM());
                //vector3D_2 = plane.Project(vector3D_2);
                //vector3D_2 = vector3D_2.Unit;

                ////calculat angle betweem two vectors
                //double angle = vector3D_1.Angle(vector3D_2);

                XYZ vectorRevit = vector3D_1.ToRevit().Normalize();

                result = Wall.Create(document, curveList, aHostObjAttributes.Id, level.Id, false, vectorRevit);

                //

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

                return result;
            }
            else if (aHostObjAttributes is FloorType)
            {
                CurveArray curveArray = new CurveArray();
                foreach (Geometry.Spatial.ICurve3D curve3D in curvable3D.GetCurves())
                    curveArray.Append(curve3D.ToRevit_Line());

                Level level = document.HighLevel(panel.LowElevation());

                result = document.Create.NewFloor(curveArray, aHostObjAttributes as FloorType, level, false);
                result.ChangeTypeId(aHostObjAttributes.Id);

                return result;

            }
            else if(aHostObjAttributes is RoofType)
            {
                List<Geometry.Spatial.ICurve3D> curve3Ds = curvable3D.GetCurves();

                CurveArray curveArray = new CurveArray();
                foreach (Geometry.Spatial.Segment3D segment3D in curve3Ds)
                    curveArray.Append(segment3D.ToRevit());

                Level level = document.HighLevel(panel.LowElevation());
                double levelElevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);

                ModelCurveArray modelCurveArray = new ModelCurveArray();
                RoofBase roofBase = document.Create.NewFootPrintRoof(curveArray, level, aHostObjAttributes as RoofType, out modelCurveArray);

                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                SlabShapeEditor slabShapeEditor = roofBase.SlabShapeEditor;
                slabShapeEditor.ResetSlabShape();

                foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                {
                    Geometry.Spatial.Point3D point3D = curve3D.GetStart();
                    if (Math.Abs(point3D.Z - levelElevation) > Geometry.Tolerance.MicroDistance)
                        slabShapeEditor.DrawPoint(point3D.ToRevit());
                }

                return roofBase;
            }

            return null;
        }
    }
}
