// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper.Revit
{
    public class SAMArchitecturalLevelInformation : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b72abb0f-6672-41e8-987b-d63fe9eaf7b9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Architectural;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalLevelInformation()
          : base("SAMArchitectural.LevelInformation", "SAMCore.LevelInformation",
              "Query Revit Level Information *use Level Picker Node",
              "SAM", "Architectural")
        {
            // GH_SAMVariableOutputParameterComponent.RegisterOutputParams clones each Param via
            // IGH_Param.Clone(), which silently resets NickName to Name for stock Grasshopper
            // param types when the two differ. The original component registered "HighLevel" with
            // a different NickName ("High Level") — restore it here after base registration
            // completes.
            int index = Params.IndexOfOutputParam("HighLevel");
            if (index != -1)
                Params.Output[index].NickName = "High Level";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_level", NickName = "_level", Description = "Revit Level", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "HighLevel", NickName = "High Level", Description = "Revit High Level", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "HighElevation", NickName = "HighElevation", Description = "High Elevation", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "Elevation", NickName = "Elevation", Description = "SAM Architectural Level Elevation", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "LowLevel", NickName = "LowLevel", Description = "Revit Low Level", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "LowElevation", NickName = "LowElevation", Description = "Low Elevation", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("_level");
            GH_ObjectWrapper objectWrapper = null;

            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Autodesk.Revit.DB.Level level = null;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot access Revit Document");
                return;
            }

            dynamic obj = objectWrapper.Value;
            if (obj is GH_Integer)
            {
                ElementId elementId = new ElementId(((GH_Integer)obj).Value);
                if (elementId == null || elementId == ElementId.InvalidElementId)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot access Element");
                    return;
                }
                level = document.GetElement(elementId) as Autodesk.Revit.DB.Level;
            }
            else if(obj is GH_String)
            {
                string @string = ((GH_String)obj).Value;
                if (@string.Length > 37)
                    @string = @string.Substring(37);

                level = document.GetElement(@string) as Autodesk.Revit.DB.Level;
            }
            else if (obj.GetType().GetProperty("Id") != null)
            {
                ElementId aId = obj.Id as ElementId;
                level = document.GetElement(aId) as Autodesk.Revit.DB.Level;
            }

            if (level == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot access Level");
                return;
            }

            if (level == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Autodesk.Revit.DB.Level level_High = Core.Revit.Query.HighLevel(level);
            double elevation_High = double.NaN;
            if (level_High != null)
            {
                elevation_High = UnitUtils.ConvertFromInternalUnits(level_High.Elevation, UnitTypeId.Meters);

            }


            Autodesk.Revit.DB.Level level_Low = Core.Revit.Query.LowLevel(level);
            double elevation_Low = double.NaN;
            if (level_Low != null)
            {
                elevation_Low = UnitUtils.ConvertFromInternalUnits(level_Low.Elevation, UnitTypeId.Meters);

            }

            index = Params.IndexOfOutputParam("HighLevel");
            if (index != -1)
                dataAccess.SetData(index, level_High);

            index = Params.IndexOfOutputParam("HighElevation");
            if (index != -1)
                dataAccess.SetData(index, new GH_Number(elevation_High));

            index = Params.IndexOfOutputParam("Elevation");
            if (index != -1)
            {
                dataAccess.SetData(index, new GH_Number(UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters)));
            }

            index = Params.IndexOfOutputParam("LowLevel");
            if (index != -1)
                dataAccess.SetData(index, level_Low);

            index = Params.IndexOfOutputParam("LowElevation");
            if (index != -1)
                dataAccess.SetData(index, new GH_Number(elevation_Low));
        }
    }
}
