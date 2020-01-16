using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Panel> ToSAM(this HostObject hostObject, Transaction transaction = null)
        {
            Construction construction = ((HostObjAttributes)hostObject.Document.GetElement(hostObject.GetTypeId())).ToSAM();

            List<Panel> result = new List<Panel>();
            foreach (IClosed3D profile in hostObject.Profiles(transaction))
                result.Add(new Panel(hostObject.Name, construction, profile));

            return result;
        }

        public static List<Panel> ToSAM(this IEnumerable<HostObject> hostObjects, Document document)
        {
            Dictionary<Document, HashSet<ElementId>> elementIdDictionary = new Dictionary<Document, HashSet<ElementId>>();
            foreach(HostObject hostObject in hostObjects)
            {
                HashSet<ElementId> elementIds;
                if (!elementIdDictionary.TryGetValue(hostObject.Document, out elementIds))
                {
                    elementIds = new HashSet<ElementId>();
                    elementIdDictionary[hostObject.Document] = elementIds;
                }

                elementIds.Add(hostObject.Id);
            }

            List<Panel> panels = new List<Panel>();
            foreach(KeyValuePair<Document, HashSet<ElementId>> keyValuePair in elementIdDictionary)
            {
                Document document_Source = keyValuePair.Key;
                if(document_Source == document)
                {
                    List<HostObject> hostObjects_Temp = keyValuePair.Value.ToList().ConvertAll(x => (HostObject)document_Source.GetElement(x));
                    foreach(HostObject hostObject in hostObjects_Temp)
                    {
                        List<Panel> panels_Temp = ToSAM(hostObject);
                        if (panels_Temp != null && panels_Temp.Count > 0)
                            panels.AddRange(panels_Temp);
                    }
                    
                }
                else
                {
                    using (Transaction transaction = new Transaction(document, "Temp"))
                    {
                        transaction.Start();
                        CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
                        IEnumerable<ElementId> elementIds = ElementTransformUtils.CopyElements(document_Source, keyValuePair.Value, document, Transform.Identity, copyPasteOptions);
                        foreach (ElementId elementId in elementIds)
                        {
                            HostObject hostObject_Copied = document.GetElement(elementId) as HostObject;
                            if (hostObject_Copied == null)
                                continue;

                            List<Panel> panels_Temp = ToSAM(hostObject_Copied, transaction);
                            if (panels_Temp != null && panels_Temp.Count > 0)
                                panels.AddRange(panels_Temp);
                        }

                        FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions().SetClearAfterRollback(true);
                        transaction.RollBack(failureHandlingOptions);
                    }
                }
            }

            return panels;
        }

        public static List<Panel> ToSAM(this RevitLinkInstance revitLinkInstance)
        {
            Document document_Source = revitLinkInstance.GetLinkDocument();

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter((new List<System.Type> { typeof(Wall), typeof(Floor), typeof(RoofBase) }).ConvertAll(x => (ElementFilter)(new ElementClassFilter(x))));

            IEnumerable<HostObject> hostObjects = new FilteredElementCollector(document_Source).WherePasses(logicalOrFilter).Cast<HostObject>();

            return ToSAM(hostObjects, revitLinkInstance.Document);
        }
    }
}
