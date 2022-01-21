using System.Collections.Generic;
using Autodesk.Revit.DB;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static Autodesk.Revit.DB.Material ToRevit(this Material material, Document document, ConvertSettings convertSettings)
        {
            if (material == null || document == null)
            {
                return null;
            }

            Autodesk.Revit.DB.Material result = convertSettings?.GetObject<Autodesk.Revit.DB.Material>(material.Guid);
            if (result != null)
            {
                return result;
            }

            List<Autodesk.Revit.DB.Material> materials = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Material)).Cast<Autodesk.Revit.DB.Material>().ToList();
            if(materials != null)
            {
                result = materials.Find(x => x.Name == material.Name);
            }

            if(result == null)
            {
               ElementId elementId = Autodesk.Revit.DB.Material.Create(document, material.Name);
                if(elementId == null || elementId == ElementId.InvalidElementId)
                {
                    return result;
                }

                result = document.GetElement(elementId) as Autodesk.Revit.DB.Material;
                if(result == null)
                {
                    return result;
                }
            }

            if(material is GasMaterial)
            {
                result.MaterialClass = "Gas";
            }
            else if(material is SolidMaterial)
            {

            }
            else if(material is LiquidMaterial)
            {
                result.MaterialClass = "Liquid";
            }

            Parameter parameter = result.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION);
            parameter.Set(material.Description);

            ElementId elementId_ThermalAsset = result.ThermalAssetId;
            if (elementId_ThermalAsset != null && elementId_ThermalAsset != ElementId.InvalidElementId)
            {
                PropertySetElement propertySetElement = document.GetElement(elementId_ThermalAsset) as PropertySetElement;
                if (propertySetElement != null)
                {
                    ThermalAsset thermalAsset = propertySetElement.GetThermalAsset();
                    if (thermalAsset != null)
                    {
                        thermalAsset.Density = material.Density;
                        thermalAsset.ThermalConductivity = material.ThermalConductivity;
                    }
                }
            }


            if (convertSettings.ConvertParameters)
            {
                Dictionary<string, object> parameters = convertSettings.GetParameters();

                Modify.SetValues(result, material);
                Modify.SetValues(result, material, ActiveSetting.Setting, parameters);
            }

            convertSettings?.Add(material.Guid, result);

            return result;
        }
    }
}