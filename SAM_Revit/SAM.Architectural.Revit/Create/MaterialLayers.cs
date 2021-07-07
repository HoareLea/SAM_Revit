using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural.Revit
{
    public static partial class Create
    {
        public static List<MaterialLayer> MaterialLayers(this Document document, CompoundStructure compoundStructure)
        {
            if (document == null || compoundStructure == null)
            {
                return null;
            }

            IEnumerable<CompoundStructureLayer> compoundStructureLayers = compoundStructure.GetLayers();
            if(compoundStructureLayers == null)
            {
                return null;
            }

            int count = compoundStructureLayers.Count();

            List<MaterialLayer> result = new List<MaterialLayer>();
            for(int i=0; i < count; i++)
            {
                CompoundStructureLayer compoundStructureLayer = compoundStructureLayers.ElementAt(i);
                if(compoundStructureLayer == null)
                {
                    continue;
                }

                Material material = document.GetElement(compoundStructureLayer.MaterialId) as Material;
                if(material == null)
                {
                    continue;
                }

                double thickness = compoundStructure.GetLayerWidth(i);

               result.Add(new MaterialLayer(material.Name, thickness));
            }

            return result;
        }
    }
}