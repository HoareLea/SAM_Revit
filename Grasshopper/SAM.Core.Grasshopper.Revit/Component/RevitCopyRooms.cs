using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper.Revit
{
    public class RevitCopyRooms : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d8c677d9-ecaa-41de-b33a-e63b46be710f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

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
        protected override ParamDefinition[] Inputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() {Name = "_revitLinkInstance", NickName = "_revitLinkInstance", Description = "Revit Link Instance", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new GooTextMapParam() { Name = "_textMap_", NickName = "_textMap_", Description = "SAM Core TextMap", Optional = true, Access = GH_ParamAccess.item }, ParamRelevance.Occasional));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_rename_", NickName = "_rename", Description = "Rename", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new ParamDefinition(param_Boolean, ParamRelevance.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new ParamDefinition(param_Boolean, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Outputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "spaces", NickName = "spaces", Description = "Revit Spaces", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Parameters Updated", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            int index_Successful = -1;

            index_Successful = Params.IndexOfOutputParam("successful");
            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, false);

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            RevitLinkInstance revitLinkInstance = null;
            index = Params.IndexOfInputParam("_revitLinkInstance");
            if (index == -1 || !dataAccess.GetData(index, ref revitLinkInstance) || revitLinkInstance == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if(levels == null || levels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(document);

            TextMap textMap = null;
            index = Params.IndexOfInputParam("_textMap_");
            if (index != -1)
                dataAccess.GetData(index, ref textMap);

            bool rename = false;
            index = Params.IndexOfInputParam("_rename");
            if (index == -1 || !dataAccess.GetData(index, ref rename))
                rename = false;

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

                    XYZ xyz = transform?.OfPoint(locationPoint.Point);
                    if (xyz == null)
                        continue;

                    double min = double.MaxValue;
                    Level level = null;
                    foreach (Level level_Temp in levels)
                    {
                        double min_Temp = Math.Abs(level_Temp.Elevation - xyz.Z);
                        if(min_Temp < min)
                        {
                            min = min_Temp;
                            level = level_Temp;
                        }
                    }

                    if (level == null)
                        continue;

                    Autodesk.Revit.DB.Mechanical.Space space = document.Create.NewSpace(level, new UV(xyz.X, xyz.Y));
                    if (space == null)
                        continue;

                    if (textMap != null)
                        Core.Revit.Modify.CopyValues(room, space, textMap);

                    if(rename)
                    {
                        string name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString();
                        if(!string.IsNullOrEmpty(name))
                        {
                            space.get_Parameter(BuiltInParameter.ROOM_NAME).Set(name);
                        }

                        string number = room.get_Parameter(BuiltInParameter.ROOM_NUMBER)?.AsString();
                        if (!string.IsNullOrEmpty(number))
                        {
                            space.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(number);
                        }
                    }

                    result.Add(space);
                }
            }

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(0, result);

            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, result != null && result.Count > 0);
        }
    }
}