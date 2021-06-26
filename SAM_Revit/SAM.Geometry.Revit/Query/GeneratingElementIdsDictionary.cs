using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Dictionary<ElementId, List<Spatial.Face3D>> GeneratingElementIdDictionary(this Element element)
        {
            GeometryElement geometryElement = element?.get_Geometry(new Options());
            if(geometryElement == null)
            {
                return null;
            }

            Dictionary<ElementId, List<Spatial.Face3D>> result = new Dictionary<ElementId, List<Spatial.Face3D>>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is Solid)
                {
                    Solid solid = geometryObject as Solid;
                    foreach (Autodesk.Revit.DB.Face face in solid.Faces)
                    {
                        ICollection<ElementId> generatingElementIds = element.GetGeneratingElementIds(face);

                        generatingElementIds.Remove(element.Id);
                        if(generatingElementIds.Count != 0)
                        {
                            List<Spatial.Face3D> face3Ds =  face.ToSAM();
                            if(face3Ds != null && face3Ds.Count != 0)
                            {
                                foreach (ElementId elementId in generatingElementIds)
                                {
                                    if (!result.TryGetValue(elementId, out List<Spatial.Face3D> face3Ds_ElementId))
                                    {
                                        face3Ds_ElementId = new List<Spatial.Face3D>();
                                        result[elementId] = face3Ds_ElementId;
                                    }

                                    face3Ds_ElementId.AddRange(face3Ds);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}