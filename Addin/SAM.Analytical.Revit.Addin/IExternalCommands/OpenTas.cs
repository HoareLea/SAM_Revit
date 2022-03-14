using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SAM.Analytical.Revit.Addin.Properties;
using SAM.Core.Revit.Addin;

namespace SAM.Analytical.Revit.Addin
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenTas : ISAMRibbonItemData
    {
        public string RibbonPanelName => "Tas";

        public int Index => 16;

        public void Create(RibbonPanel ribbonPanel)
        {
            SplitButtonData splitButtonData = new SplitButtonData(Core.Query.FullTypeName(GetType()), "OpenTAS");
            SplitButton splitButton = ribbonPanel.AddItem(splitButtonData) as SplitButton;

            PushButtonData pushButtonData_T3D = new PushButtonData(Core.Query.FullTypeName(typeof(OpenT3D)), "T3D", GetType().Assembly.Location, typeof(OpenT3D).FullName);
            pushButtonData_T3D.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_T3D, 32, 32);
            pushButtonData_T3D.ToolTip = "Open TAS 3D Modeler";
            pushButtonData_T3D.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_T3D);


            PushButtonData pushButtonData_TBD = new PushButtonData(Core.Query.FullTypeName(typeof(OpenTBD)), "TBD", GetType().Assembly.Location, typeof(OpenTBD).FullName);
            pushButtonData_TBD.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_TBD, 32, 32);
            pushButtonData_TBD.ToolTip = "Open TAS Building Designer";
            pushButtonData_TBD.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_TBD);

            PushButtonData pushButtonData_TSD = new PushButtonData(Core.Query.FullTypeName(typeof(OpenTSD)), "TSD", GetType().Assembly.Location, typeof(OpenTSD).FullName);
            pushButtonData_TSD.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_TSD, 32, 32);
            pushButtonData_TSD.ToolTip = "Open TAS Results";
            pushButtonData_TSD.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_TSD);

            PushButtonData pushButtonData_TPD = new PushButtonData(Core.Query.FullTypeName(typeof(OpenTPD)), "TPD", GetType().Assembly.Location, typeof(OpenTPD).FullName);
            pushButtonData_TPD.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_TPD, 32, 32);
            pushButtonData_TPD.ToolTip = "Open TAS Systems";
            pushButtonData_TPD.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_TPD);

            PushButtonData pushButtonData_PMVAndPPD = new PushButtonData(Core.Query.FullTypeName(typeof(OpenPMVAndPPD)), "PMV & PPD", GetType().Assembly.Location, typeof(OpenPMVAndPPD).FullName);
            pushButtonData_PMVAndPPD.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_PMVAndPPV, 32, 32);
            pushButtonData_PMVAndPPD.ToolTip = "Open PMV And PPD";
            pushButtonData_PMVAndPPD.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_PMVAndPPD);

            PushButtonData pushButtonData_UKBRStudio2013 = new PushButtonData(Core.Query.FullTypeName(typeof(OpenUKBRStudio2013)), "UK PartL 2013", GetType().Assembly.Location, typeof(OpenUKBRStudio2013).FullName);
            pushButtonData_UKBRStudio2013.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_UKBRStudio2013, 32, 32);
            pushButtonData_UKBRStudio2013.ToolTip = "Open UK PartL 2013 Studio";
            pushButtonData_UKBRStudio2013.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_UKBRStudio2013);

            PushButtonData pushButtonData_NPOStudio = new PushButtonData(Core.Query.FullTypeName(typeof(OpenNPOStudio)), "ASHRAE 90.1", GetType().Assembly.Location, typeof(OpenNPOStudio).FullName);
            pushButtonData_NPOStudio.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_NPOStudio, 32, 32);
            pushButtonData_NPOStudio.ToolTip = "Open NPO Studio (ASHRAE 90.1)";
            pushButtonData_NPOStudio.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_NPOStudio);

            PushButtonData pushButtonData_IDFWizard = new PushButtonData(Core.Query.FullTypeName(typeof(OpenIDFWizard)), "IDF", GetType().Assembly.Location, typeof(OpenIDFWizard).FullName);
            pushButtonData_IDFWizard.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_IDFWizard, 32, 32);
            pushButtonData_IDFWizard.ToolTip = "Open IDF Wizard";
            pushButtonData_IDFWizard.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_IDFWizard);
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenT3D : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.TAS3DPath();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenTBD : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.TBDPath();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenTSD : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.TSDPath();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenTPD : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.TPDPath();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenPMVAndPPD : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.PMVAndPPDPath();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenUKBRStudio2013 : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.UKBRStudio2013Path();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenNPOStudio : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.NPOStudioPath();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class OpenIDFWizard : IExternalCommand
    {
        public Result Execute(ExternalCommandData ExternalCommandData, ref string Message, ElementSet Elements)
        {
            string path = Core.Tas.Query.IDFWizardPath();

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return Result.Failed;
            }

            System.Diagnostics.Process.Start(path);

            return Result.Succeeded;
        }
    }
}
