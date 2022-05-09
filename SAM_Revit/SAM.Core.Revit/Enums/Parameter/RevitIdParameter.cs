using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core.Revit
{
    [AssociatedTypes(typeof(IntegerId)), Description("Revit Id Parameter")]
    public enum RevitIdParameter
    {
        [ParameterProperties("Full Name", "Revit Full Name"), ParameterValue(ParameterType.String)] FullName,
        [ParameterProperties("Category Name", "Revit Category Name"), ParameterValue(ParameterType.String)] CategoryName,
        [ParameterProperties("Unique Id", "Revit Unique Id"), ParameterValue(ParameterType.String)] UniqueId,
    }
}
