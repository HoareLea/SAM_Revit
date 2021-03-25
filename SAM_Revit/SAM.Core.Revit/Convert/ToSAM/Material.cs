using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static Material ToSAM(this Autodesk.Revit.DB.Material material, ConvertSettings convertSettings)
        {
            if (material == null)
                return null;

            Material result = convertSettings?.GetObject<Material>(material.Id);
            if (result != null)
                return result;

            Document document = material.Document;

            double density = double.NaN;
            double thermalConductivity = double.NaN;
            ThermalMaterialType thermalMaterialType = ThermalMaterialType.Undefined;
            bool transmitsLight = false;

            ElementId elementId = material.ThermalAssetId;
            if (elementId != null && elementId != ElementId.InvalidElementId)
            {
                PropertySetElement propertySetElement = document.GetElement(elementId) as PropertySetElement;
                if (propertySetElement != null)
                {
                    ThermalAsset thermalAsset = propertySetElement.GetThermalAsset();
                    if (thermalAsset != null)
                    {
                        density = thermalAsset.Density;
                        thermalConductivity = thermalAsset.ThermalConductivity;
                        thermalMaterialType = thermalAsset.ThermalMaterialType;
                        transmitsLight = thermalAsset.TransmitsLight;
                    }
                }
            }

            string description = material.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)?.AsString();

            if (thermalMaterialType != ThermalMaterialType.Undefined)
            {
                switch (thermalMaterialType)
                {
                    case ThermalMaterialType.Gas:
                        result = new GasMaterial(System.Guid.NewGuid(), material.Name, material.Name, description, thermalConductivity, density, double.NaN, double.NaN);
                        break;
                    case ThermalMaterialType.Liquid:
                        result = new LiquidMaterial(System.Guid.NewGuid(), material.Name, material.Name, description, thermalConductivity, density, double.NaN, double.NaN);
                        break;
                    case ThermalMaterialType.Solid:
                        if (transmitsLight)
                            result = new TransparentMaterial(material.Name, null, material.Name, description, thermalConductivity, double.NaN, density);
                        else
                            result = new OpaqueMaterial(material.Name, null, material.Name, description, thermalConductivity, double.NaN, density);
                        break;
                }
            }

            if(result == null)
            {
                string materialClass = material.MaterialClass?.Trim();
                if (string.IsNullOrWhiteSpace(materialClass))
                {
                    switch (materialClass.ToLower())
                    {
                        case "glass":
                            result = new TransparentMaterial(material.Name, materialClass, material.Name, description, thermalConductivity, double.NaN, density);
                            break;
                        case "gas":
                            result = new GasMaterial(System.Guid.NewGuid(), material.Name, material.Name, description, thermalConductivity, density, double.NaN, double.NaN);
                            break;
                        case "ceramic":
                        case "earth":
                        case "brass":
                        case "metal":
                        case "wood":
                        case "concrete":
                        case "masonry":
                        case "paint":
                        case "paint/coating":
                        case "plastic":
                        case "stone":
                        case "textile":
                            result = new OpaqueMaterial(material.Name, materialClass, material.Name, description, thermalConductivity, double.NaN, density);
                            break;
                        case "liquid":
                            result = new LiquidMaterial(System.Guid.NewGuid(), material.Name, material.Name, description, thermalConductivity, density, double.NaN, double.NaN);
                            break;
                    }
                }
            }

            if (result == null)
            {
                int transparency = material.Transparency;
                if (transparency < 10)
                    result = new OpaqueMaterial(material.Name, null, material.Name, description, thermalConductivity, double.NaN, density);
                else if (transparency < 100)
                    result = new TransparentMaterial(material.Name, null, material.Name, description, thermalConductivity, double.NaN, density);
                else
                    result = new GasMaterial(System.Guid.NewGuid(), material.Name, material.Name, description, thermalConductivity, double.NaN, density, double.NaN);
            }

            if(result != null)
            {
                convertSettings?.Add(material.Id, result);
            }
            

            return result;
        }
    }
}