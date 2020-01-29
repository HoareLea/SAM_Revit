using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Grasshopper.Kernel;
using RhinoInside;
using RhinoInside.Revit.GH;

using Autodesk.Revit.DB;

using SAM.Analytical.Grasshopper.Revit.Properties;


namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalRevit_Test :  RhinoInside.Revit.GH.Components.ReconstructElementComponent
    {
        private List<Wall> joinedWalls = new List<Wall>();

        private static readonly FailureDefinitionId[] failureDefinitionIdsToFix = new FailureDefinitionId[]
        {
            BuiltInFailures.CreationFailures.CannotDrawWallsError,
            BuiltInFailures.JoinElementsFailures.CannotJoinElementsError,
        };

        protected override IEnumerable<FailureDefinitionId> FailureDefinitionIdsToFix => failureDefinitionIdsToFix;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRevit_Test()
          : base("SAMAnalytical.Revit_Test", "SAManalytical.Revit",
              "Create Revit HostObject from SAM Analytical ie. Panel",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObject(), "HostObjects", "hso" ,"HostObjects", GH_ParamAccess.item);
        }

        protected override void OnAfterStart(Document document, string strTransactionName)
        {
            base.OnAfterStart(document, strTransactionName);

            // Disable all previous walls joins
            if (PreviousStructure is object)
            {
                var unjoinedWalls = PreviousStructure.OfType<RhinoInside.Revit.GH.Types.Element>().
                                    Select(x => document.GetElement(x)).
                                    OfType<Wall>().
                                    Where(x => x.Pinned).
                                    Select
                                    (
                                      x => Tuple.Create
                                      (
                                        x,
                                        (x.Location as LocationCurve).get_JoinType(0),
                                        (x.Location as LocationCurve).get_JoinType(1)
                                      )
                                    ).
                                    ToArray();

                foreach (var unjoinedWall in unjoinedWalls)
                {
                    var location = unjoinedWall.Item1.Location as LocationCurve;
                    if (WallUtils.IsWallJoinAllowedAtEnd(unjoinedWall.Item1, 0))
                    {
                        WallUtils.DisallowWallJoinAtEnd(unjoinedWall.Item1, 0);
                        WallUtils.AllowWallJoinAtEnd(unjoinedWall.Item1, 0);
                        location.set_JoinType(0, unjoinedWall.Item2);
                    }

                    if (WallUtils.IsWallJoinAllowedAtEnd(unjoinedWall.Item1, 1))
                    {
                        WallUtils.DisallowWallJoinAtEnd(unjoinedWall.Item1, 1);
                        WallUtils.AllowWallJoinAtEnd(unjoinedWall.Item1, 1);
                        location.set_JoinType(1, unjoinedWall.Item3);
                    }
                }
            }
        }

        protected override void OnBeforeCommit(Document document, string strTransactionName)
        {
            base.OnBeforeCommit(document, strTransactionName);

            // Reenable new joined walls
            foreach (var wallToJoin in joinedWalls)
            {
                WallUtils.AllowWallJoinAtEnd(wallToJoin, 0);
                WallUtils.AllowWallJoinAtEnd(wallToJoin, 1);
            }

            joinedWalls = new List<Wall>();
        }

        private void ReconstructSAMAnalyticalRevit_Test(
            Document doc, 
            ref Wall element, 
            
            Rhino.Geometry.Curve curve, 
            Optional<WallType> type,
            Optional<Level> level,
            [Optional] bool structural,
            [Optional] double height,
            [Optional] WallLocationLine locationLine,
            [Optional] bool flipped,
            [Optional, NickName("J")] bool allowJoins
        )
        {
            var scaleFactor = 1.0 / RhinoInside.Revit.Revit.ModelUnits;

            SolveOptionalType(ref type, doc, ElementTypeGroup.WallType, nameof(type));

            double axisMinZ = Math.Min(curve.PointAtStart.Z, curve.PointAtEnd.Z);
            bool levelIsEmpty = SolveOptionalLevel(ref level, doc, curve, nameof(level));

            height *= scaleFactor;
            if (height < RhinoInside.Revit.Revit.VertexTolerance)
                height = type.GetValueOrDefault()?.GetCompoundStructure()?.SampleHeight ?? LiteralLengthValue(6.0) / RhinoInside.Revit.Revit.ModelUnits;

            // Axis
            var levelPlane = new Rhino.Geometry.Plane(new Rhino.Geometry.Point3d(0.0, 0.0, level.Value.Elevation), Rhino.Geometry.Vector3d.ZAxis);
            curve = Rhino.Geometry.Curve.ProjectToPlane(curve, levelPlane);

            Curve centerLine = null;//curve.ToHost();

            // LocationLine
            if (locationLine != WallLocationLine.WallCenterline)
            {
                double offsetDist = 0.0;
                var compoundStructure = type.Value.GetCompoundStructure();
                if (compoundStructure == null)
                {
                    switch (locationLine)
                    {
                        case WallLocationLine.WallCenterline:
                        case WallLocationLine.CoreCenterline:
                            break;
                        case WallLocationLine.FinishFaceExterior:
                        case WallLocationLine.CoreExterior:
                            offsetDist = type.Value.Width / +2.0;
                            break;
                        case WallLocationLine.FinishFaceInterior:
                        case WallLocationLine.CoreInterior:
                            offsetDist = type.Value.Width / -2.0;
                            break;
                    }
                }
                else
                {
                    if (!compoundStructure.IsVerticallyHomogeneous())
                        compoundStructure = CompoundStructure.CreateSimpleCompoundStructure(compoundStructure.GetLayers());

                    offsetDist = compoundStructure.GetOffsetForLocationLine(locationLine);
                }

                if (offsetDist != 0.0)
                    centerLine = centerLine.CreateOffset(flipped ? -offsetDist : offsetDist, XYZ.BasisZ);
            }

            // Type
            ChangeElementTypeId(ref element, type.Value.Id);

            Wall newWall = null;
            if (element is Wall && ((Wall)element).Location is LocationCurve)
            {
                newWall = (Wall)element;
                ((LocationCurve)newWall.Location).Curve = centerLine;
            }
            else
            {
                newWall = Wall.Create
                (
                  doc,
                  centerLine,
                  type.Value.Id,
                  level.Value.Id,
                  height,
                  levelIsEmpty ? axisMinZ - level.Value.Elevation : 0.0,
                  flipped,
                  structural
                );

                // Walls are created with the last LocationLine used in the Revit editor!!
                //newWall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set((int) WallLocationLine.WallCenterline);

                var parametersMask = new BuiltInParameter[]
                {
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM,
                    BuiltInParameter.WALL_BASE_CONSTRAINT,
                    BuiltInParameter.WALL_USER_HEIGHT_PARAM,
                    BuiltInParameter.WALL_BASE_OFFSET,
                    BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT,
                    BuiltInParameter.WALL_KEY_REF_PARAM
                };

                ReplaceElement(ref element, newWall, parametersMask);
            }

            if (newWall != null)
            {
                newWall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).Set(level.Value.Id);
                newWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(levelIsEmpty ? axisMinZ - level.Value.Elevation : 0.0);
                newWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).Set(height);
                newWall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).Set(structural ? 1 : 0);
                newWall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set((int)locationLine);

                if (newWall.Flipped != flipped)
                    newWall.Flip();

                // Setup joins in a last step
                if (allowJoins) joinedWalls.Add(newWall);
                else
                {
                    WallUtils.DisallowWallJoinAtEnd(newWall, 0);
                    WallUtils.DisallowWallJoinAtEnd(newWall, 1);
                }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SAM_Revit;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("222e0a64-9514-47d3-98ac-72e95124841d"); }
        }
    }
}