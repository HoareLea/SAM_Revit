using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper.Revit
{
    public class RevitSAMGeometryAdaptiveComponentPoint3Ds : GH_SAMVariableOutputParameterComponent
    {       
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("33777cb3-aa9d-4f28-a585-fd2d4f5026cc");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSAMGeometryAdaptiveComponentPoint3Ds()
          : base("Revit.SAMGeometryAdaptiveComponentPoint3Ds", "Revit.SAMGeometryAdaptiveComponentPoint3Ds",
              "Gets SAM Geometry Point3Ds for Revit Adaptive Component",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.FamilyInstance() { Name = "_familyInstance", NickName = "_familyInstance", Description = "Revit FamilyInstance", Access = GH_ParamAccess.item}, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "point3Ds", NickName = "point3Ds", Description = "SAM Geometry Point3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "point3Ds_Placement", NickName = "point3Ds_Placement", Description = "SAM Geometry Placement Point3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "point3Ds_ShapeHandle", NickName = "point3Ds_ShapeHandle", Description = "SAM Geometry Shape Handle Point3Ds", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            GH_ObjectWrapper objectWrapper;

            Document document = null;

            FamilyInstance familyInstance = null;
            index = Params.IndexOfInputParam("document_");
            if (index == -1 || !dataAccess.GetData(index, ref familyInstance) || familyInstance == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Geometry.Revit.Query.TryGetAdaptiveComponentPoint3Ds(familyInstance, out List<Point3D> point3Ds, out List<Point3D> point3Ds_Placement, out List<Point3D> point3Ds_ShapeHandle);

            index = Params.IndexOfOutputParam("point3Ds");
            if(index != -1)
            {
                dataAccess.SetDataList(index, point3Ds);
            }

            index = Params.IndexOfOutputParam("point3Ds_Placement");
            if (index != -1)
            {
                dataAccess.SetDataList(index, point3Ds_Placement);
            }

            index = Params.IndexOfOutputParam("point3Ds_ShapeHandle");
            if (index != -1)
            {
                dataAccess.SetDataList(index, point3Ds_ShapeHandle);
            }
        }
    }
}