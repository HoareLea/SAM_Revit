using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core.Revit
{
    [AssociatedTypes(typeof(LongId)), Description("Revit Id Parameter")]
    public enum RevitIdParameter
    {
        [ParameterProperties("Full Name", "Revit Full Name"), ParameterValue(ParameterType.String)] FullName,
        [ParameterProperties("Category Name", "Revit Category Name"), ParameterValue(ParameterType.String)] CategoryName,
        [ParameterProperties("Category Id", "Revit Category Id"), ParameterValue(ParameterType.Integer)] CategoryId,
        [ParameterProperties("Unique Id", "Revit Unique Id"), ParameterValue(ParameterType.String)] UniqueId,
    }
}
