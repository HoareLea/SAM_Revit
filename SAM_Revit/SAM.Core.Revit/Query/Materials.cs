using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<Autodesk.Revit.DB.Material> Materials(this Element element, bool returnPaintMaterials)
        {
            IEnumerable<ElementId> elementIds = element?.GetMaterialIds(returnPaintMaterials);
            if(elementIds == null)
            {
                return null;
            }

            Document document = element.Document;

            List<Autodesk.Revit.DB.Material> result = new List<Autodesk.Revit.DB.Material>();
            foreach(ElementId elementId in elementIds)
            {
                Autodesk.Revit.DB.Material material = document.GetElement(elementId) as Autodesk.Revit.DB.Material;
                if(material != null)
                {
                    result.Add(material);
                }
            }

            return result;
        }
    }
}