using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static OpeningType ToSAM_OpeningType(this FamilySymbol familySymbol, ConvertSettings convertSettings)
        {
            if (familySymbol == null)
            {
                return null;
            }


            OpeningType result = convertSettings?.GetObject<OpeningType>(familySymbol.Id);
            if (result != null)
            {
                return result;
            }

            string name = familySymbol.Name;


#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            switch ((BuiltInCategory)familySymbol.Category.Id.IntegerValue)
#else
            switch ((BuiltInCategory)familySymbol.Category.Id.Value)
#endif
            {
                case BuiltInCategory.OST_Windows:
                case BuiltInCategory.OST_CurtainWallPanels:
                    result = new WindowType(name);
                    break;

                case BuiltInCategory.OST_Doors:
                    result = new DoorType(name);
                    break;
            }

            if(result == null)
            {
                return null;
            }

            Dictionary<ElementId, List<Shell>> materialElementIdsDictionary = Geometry.Revit.Query.MaterialElementIdsDictionary(familySymbol);
            if(materialElementIdsDictionary != null && materialElementIdsDictionary.Count != 0)
            {
                Document document = familySymbol.Document;

                Material material_Transparent = null;
                double volume_Transparent = double.MinValue;
                double thickness_Transparent = 0;

                Material material_Opaque = null;
                double volume_Opaque = double.MinValue;
                double thickness_Opaque = 0;


                foreach (KeyValuePair<ElementId, List<Shell>> keyValuePair in materialElementIdsDictionary)
                {
                    Material material = document.GetElement(keyValuePair.Key) as Material;
                    if(material == null)
                    {
                        continue;
                    }

                    double volume = keyValuePair.Value.ConvertAll(x => x.Volume()).Sum();
                    if(Core.Revit.Query.IsTransparent(material))
                    {
                        if(volume_Transparent < volume)
                        {
                            volume_Transparent = volume;
                            material_Transparent = material;
                            thickness_Transparent = Geometry.Spatial.Query.MinDimension(keyValuePair.Value.ConvertAll(x => x.GetBoundingBox()));
                        }
                    }
                    else
                    {
                        if (volume_Opaque < volume)
                        {
                            volume_Opaque = volume;
                            material_Opaque = material;
                            thickness_Opaque = Geometry.Spatial.Query.MinDimension(keyValuePair.Value.ConvertAll(x => x.GetBoundingBox()));
                        }

                    }
                }

                if(material_Transparent != null)
                {
                    List<Architectural.MaterialLayer> materialLayers = new List<Architectural.MaterialLayer>();
                    materialLayers.Add(new Architectural.MaterialLayer(material_Transparent.Name, thickness_Transparent));
                    result.PaneMaterialLayers = materialLayers;
                }

                if (material_Opaque != null)
                {
                    List<Architectural.MaterialLayer> materialLayers = new List<Architectural.MaterialLayer>();
                    materialLayers.Add(new Architectural.MaterialLayer(material_Opaque.Name, thickness_Opaque));
                    result.FrameMaterialLayers = materialLayers;
                }
            }

            result.UpdateParameterSets(familySymbol);

            convertSettings?.Add(familySymbol.Id, result);

            return result;
        }
    }
}