using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Autodesk.Revit.DB;

using SAM.Analytical.Grasshopper.Revit.Properties;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitCreateHostObject : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitCreateHostObject()
          : base("SAManalytical.Revit", "SAManalytical.Revit",
              "Create Revit HostObject from SAM Analytical ie. Panel",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("Document", "Document", "Revit Document", GH_ParamAccess.item);
            inputParamManager.AddGenericParameter("Panels", "Pnls", "SAM Analytical ie. Panel", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("HostObjects", "HostObjects", "HostObjects", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Autodesk.Revit.DB.Document document = objectWrapper.Value as Autodesk.Revit.DB.Document;
            if(document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<GH_ObjectWrapper> objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(1, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panelList = new List<Panel>();
            foreach (GH_ObjectWrapper objectWrapper_Temp in objectWrapperList)
            {
                Panel panel = objectWrapper_Temp.Value as Panel;
                if (panel != null)
                    panelList.Add(panel);
            }

            Level aLevel = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().First();
            Wall wall = null;
            using (Transaction aTransaction = new Transaction(document, this.GetType().Name))
            {
                aTransaction.Start();
                wall = Wall.Create(document, Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)), aLevel.Id, false);
                aTransaction.Commit();
            }

            if (wall != null)
                dataAccess.SetDataList(0, new List<object>() { wall });//{ RhinoInside.Revit.GH.Types.Element.FromElement(wall)});
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
            get { return new Guid("27b09cfd-97fc-44a2-b71e-bcfd6cf45890"); }
        }
    }
}