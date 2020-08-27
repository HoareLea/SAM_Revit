using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMApertureRevit : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5c1fbd67-9406-4872-b679-49faa4d1132b");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMApertureRevit()
          : base("SAMAperture.Revit", "SAMAperture.Revit",
              "Convert SAM Aperture to Revit Windnow/Door",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooApertureParam(), "_aperture", "_aperture", "SAM Analytical Aperture", GH_ParamAccess.item);

            int index = inputParamManager.AddGenericParameter("_convertSettings_", "_convertSettings_", "SAM Convert Settings", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.FamilyInstance(), "FamilyInstance", "FamilyInstance", "Revit FamilyInstance", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(2, ref run) || !run)
                return;

            ConvertSettings convertSettings = null;
            dataAccess.GetData(1, ref convertSettings);
            convertSettings = this.UpdateSolutionEndEventHandler(convertSettings);

            Aperture aperture = null;
            if (!dataAccess.GetData(0, ref aperture))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            if (convertSettings.RemoveExisting)
            {
                ElementId elementId = aperture.ElementId();
                if (elementId != null && elementId != ElementId.InvalidElementId)
                {
                    Element element = document.GetElement(elementId) as SpatialElement;
                    if (element != null)
                        document.Delete(elementId);
                }
            }

            HostObject hostObject = null;

            PanelGroup panelGroup = Analytical.Query.PanelGroup(Analytical.Query.PanelType(aperture.Plane.Normal));
            switch(panelGroup)
            {
                case PanelGroup.Wall:
                    IEnumerable<Wall> walls = Geometry.Revit.Query.Elements<Wall>(document, aperture.GetBoundingBox());
                    if (walls != null && walls.Count() != 0)
                        hostObject = walls.First();
                    break;
                case PanelGroup.Floor:
                    IEnumerable<Element> elements_Floor = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), BuiltInCategory.OST_Floors);
                    if (elements_Floor != null && elements_Floor.Count() != 0)
                        hostObject = elements_Floor.First() as HostObject;
                    break;
                case PanelGroup.Roof:
                    IEnumerable<Element> elements_Roof = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), BuiltInCategory.OST_Roofs);
                    if (elements_Roof != null && elements_Roof.Count() != 0)
                        hostObject = elements_Roof.First() as HostObject;
                    break;
            }    
            
            if(hostObject == null)
            {
                switch(panelGroup)
                {
                    case PanelGroup.Roof:
                        IEnumerable<Element> elements_Floor = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), BuiltInCategory.OST_Floors);
                        if (elements_Floor != null && elements_Floor.Count() != 0)
                            hostObject = elements_Floor.First() as HostObject;
                        break;
                    case PanelGroup.Floor:
                        IEnumerable<Element> elements_Roof = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), BuiltInCategory.OST_Roofs);
                        if (elements_Roof != null && elements_Roof.Count() != 0)
                            hostObject = elements_Roof.First() as HostObject;
                        break;
                }
            }

            if(hostObject == null)
            {
                dataAccess.SetData(0, null);
                return;
            }

            FamilyInstance familyInstance_Revit = Analytical.Revit.Convert.ToRevit(aperture, document, hostObject,convertSettings);

            dataAccess.SetData(0, familyInstance_Revit);
        }
    }
}