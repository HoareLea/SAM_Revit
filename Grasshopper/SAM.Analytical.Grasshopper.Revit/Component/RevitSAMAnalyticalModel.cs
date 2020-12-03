using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitSAMAnalyticalModel : SAMTransactionalComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fba18b3c-3778-44aa-8ea5-4cc4a783cc07");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        protected override ParamDefinition[] Inputs
        {
            get
            {
                ParamDefinition[] paramDefinitions = new ParamDefinition[1];

                Param_Boolean param_Boolean = new Param_Boolean();
                param_Boolean.Access = GH_ParamAccess.item;
                paramDefinitions[0] = ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding, false);

                return paramDefinitions;
            }
        }

        protected override ParamDefinition[] Outputs
        {
            get
            {
                ParamDefinition[] paramDefinitions = new ParamDefinition[2];

                GooAnalyticalModelParam gooAnalyticalModelParam = new GooAnalyticalModelParam();
                gooAnalyticalModelParam.Access = GH_ParamAccess.item;
                paramDefinitions[0] = ParamDefinition.FromParam(gooAnalyticalModelParam);

                Param_Boolean param_Boolean = new Param_Boolean();
                param_Boolean.Name = "Successful";
                param_Boolean.NickName = "Successful";
                param_Boolean.Description = "Correctly imported?";
                param_Boolean.Access = GH_ParamAccess.item;
                paramDefinitions[1] = ParamDefinition.FromParam(param_Boolean);

                return paramDefinitions;
            }
        }

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSAMAnalyticalModel()
          : base("Revit.SAMAnalyticalModel", "Revit.SAMAnalyticalModel",
              "Convert Revit To SAM Analytical Model, using Revit gbXML Analytical model generation",
              "SAM", "Revit")
        {
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(1, false);

            bool run = false;
            if (!dataAccess.GetData(0, ref run) || !run)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            AnalyticalModel analyticalModel = null;

            using (Transaction transaction = new Transaction(document, "GetAnalyticalModel"))
            {
                FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
                failureHandlingOptions.SetFailuresPreprocessor(new WarningSwallower());
                transaction.SetFailureHandlingOptions(failureHandlingOptions);

                transaction.Start();

                ConvertSettings convertSettings = new ConvertSettings(true, true, true);
                try
                {
                    analyticalModel = Analytical.Revit.Convert.ToSAM_AnalyticalModel(document, convertSettings);
                }
                catch(Exception exception)
                {
                    if(exception.Message.Contains("spatial bounding elements"))
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check your setting in Export Category: Rooms/Spaces in Export gbXML settings");
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, exception.Message);
                }
                

                transaction.RollBack();
            }

            dataAccess.SetData(0, new GooAnalyticalModel(analyticalModel));
            dataAccess.SetData(1, analyticalModel != null);
        }
    }
}