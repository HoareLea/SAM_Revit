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
            pushButtonData_T3D.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_T3D);
            pushButtonData_T3D.ToolTip = "Open TAS 3D Modeler";
            pushButtonData_T3D.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_T3D);


            PushButtonData pushButtonData_TBD = new PushButtonData(Core.Query.FullTypeName(typeof(OpenTBD)), "TBD", GetType().Assembly.Location, typeof(OpenTBD).FullName);
            pushButtonData_TBD.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_TBD);
            pushButtonData_TBD.ToolTip = "Open TAS Building Designer";
            pushButtonData_TBD.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_TBD);

            PushButtonData pushButtonData_TSD = new PushButtonData(Core.Query.FullTypeName(typeof(OpenTSD)), "TSD", GetType().Assembly.Location, typeof(OpenTSD).FullName);
            pushButtonData_TSD.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_TSD);
            pushButtonData_TSD.ToolTip = "Open TAS Results";
            pushButtonData_TSD.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_TSD);

            PushButtonData pushButtonData_TPD = new PushButtonData(Core.Query.FullTypeName(typeof(OpenTPD)), "TPD", GetType().Assembly.Location, typeof(OpenTPD).FullName);
            pushButtonData_TPD.LargeImage = Core.Windows.Convert.ToBitmapSource(Resources.SAM_TPD);
            pushButtonData_TPD.ToolTip = "Open TAS Systems";
            pushButtonData_TPD.AvailabilityClassName = typeof(AlwaysAvailableExternalCommandAvailability).FullName;
            splitButton.AddPushButton(pushButtonData_TPD);
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
}
