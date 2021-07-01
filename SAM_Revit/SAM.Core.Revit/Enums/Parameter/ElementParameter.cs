using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core.Revit
{
    [AssociatedTypes(typeof(SAMObject)), Description("Revit Element Parameter")]
    public enum ElementParameter
    {
        [ParameterProperties("Element Id", "Integer value from ElementId of Revit Element"), ParameterValue(ParameterType.Integer)] ElementId,
        [ParameterProperties("Unique Id", "UniqueId property of Revit Element"), ParameterValue(ParameterType.String)] UniqueId,
        [ParameterProperties("Category Name", "Category Name of Revit Element"), ParameterValue(ParameterType.String)] CategoryName,
        [ParameterProperties("Reference", "Stable Reference Representation"), ParameterValue(ParameterType.String)] Reference,
    }
}
