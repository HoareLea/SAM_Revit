using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

namespace SAM.Core.Revit.Addin
{
    public abstract class PushButtonExternalCommand : IExternalCommand, ISAMRibbonItemData
    { 
        public abstract string RibbonPanelName { get; }

        public abstract int Index { get; }

        public abstract BitmapSource BitmapSource { get; }

        public abstract string Text { get; }

        public abstract string ToolTip { get; }

        public abstract string AvailabilityClassName { get; }

        public abstract Autodesk.Revit.UI.Result Execute(ExternalCommandData externalCommandData, ref string message, ElementSet elementSet);

        public virtual void Create(RibbonPanel ribbonPanel)
        {
            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData(Query.FullTypeName(GetType()), Text, GetType().Assembly.Location, GetType().FullName)) as PushButton;
            pushButton.ToolTip = ToolTip;
            pushButton.LargeImage = BitmapSource;
            pushButton.Image = BitmapSource;
            if (!string.IsNullOrWhiteSpace(AvailabilityClassName))
            {
                pushButton.AvailabilityClassName = AvailabilityClassName;
            }
        }
    }
}
