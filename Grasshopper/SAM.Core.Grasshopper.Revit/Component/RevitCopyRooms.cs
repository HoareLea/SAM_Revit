using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper.Revit
{
    public class RevitCopyRooms : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d8c677d9-ecaa-41de-b33a-e63b46be710f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitCopyRooms()
          : base("Revit.CopyRooms", "Revit.CopyRooms",
              "Copy Rooms from linked Revit model",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "_revitLinkedModel", "_revitLinkedModel", "SAM Revit Linked Model", GH_ParamAccess.item);

            GooTextMapParam gooTextMapParam = new GooTextMapParam() { Name = "_textMap_", NickName = "_textMap_", Description = "SAM Core TextMap", Optional = true, Access = GH_ParamAccess.item };
            inputParamManager.AddParameter(gooTextMapParam);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.SpatialElement(), "Spaces", "Spaces", "Revit Spaces", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Parameters Updated", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(1, false);

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            RevitLinkInstance revitLinkInstance = null;
            if (!dataAccess.GetData(0, ref revitLinkInstance) || revitLinkInstance == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Level> levels = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Levels).Cast<Level>().ToList();
            if(levels == null || levels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            TextMap textMap = null;
            dataAccess.GetData(1, ref textMap);


            Transform transform = revitLinkInstance.GetTotalTransform();
            Document document_Linked = revitLinkInstance.GetLinkDocument();


            List<Autodesk.Revit.DB.Mechanical.Space> result = new List<Autodesk.Revit.DB.Mechanical.Space>();

            List<Autodesk.Revit.DB.Architecture.Room> rooms = new FilteredElementCollector(document_Linked).OfCategory(BuiltInCategory.OST_Rooms).Cast<Autodesk.Revit.DB.Architecture.Room>().ToList();
            if(rooms != null && rooms.Count > 0)
            {
                foreach(Autodesk.Revit.DB.Architecture.Room room in rooms)
                {
                    LocationPoint locationPoint = room?.Location as LocationPoint;
                    if (locationPoint == null)
                        continue;

                    XYZ xyz = transform?.Inverse?.OfPoint(locationPoint.Point);
                    if (xyz == null)
                        continue;

                    levels.Sort((x, y) => (x.Elevation - xyz.Z).CompareTo(y.Elevation - xyz.Z));
                    Level level = levels[0];

                    Autodesk.Revit.DB.Mechanical.Space space = document.Create.NewSpace(level, new UV(xyz.X, xyz.Y));
                    if (space == null)
                        continue;

                    if (textMap != null)
                        Core.Revit.Modify.CopyValues(room, space, textMap);
                }
            }

            dataAccess.SetDataList(0, result);
            dataAccess.SetData(1, result != null && result.Count > 0);
        }
    }
}