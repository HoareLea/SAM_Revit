using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public class PurgeMaterialsWrapper
    {
        private Document document;

        private bool checkElements;
        private bool purge;

        public PurgeMaterialsWrapper(Document document)
        {
            this.document = document;
        }

        private void Initialize()
        {
            checkElements = false;
            purge = false;
        }

        public Dictionary<ElementId, string> Purge()
        {
            if(document == null)
            {
                return null;
            }

            return Purge(new FilteredElementCollector(document).OfClass(typeof(Material)).ToElementIds()?.ToArray());
        }

        public Dictionary<ElementId, string>Purge(params ElementId[] elementIds)
        {
            if(elementIds == null)
            {
                return null;
            }

            Dictionary<ElementId, string> result = new Dictionary<ElementId, string>();

            if (elementIds.Length == 0)
            {
                return result;
            }

            Application application = document.Application;
            application.DocumentChanged += Application_DocumentChanged;

            foreach (ElementId elementId in elementIds)
            {
                string name = document.GetElement(elementId)?.Name;

                using (TransactionGroup transactionGroup = new TransactionGroup(document, "Purge Materials"))
                {
                    transactionGroup.Start();
                    using (Transaction transaction = new Transaction(document, "Purge Material"))
                    {
                        transaction.Start();
                        checkElements = true;
                        document.Delete(elementId);
                        transaction.Commit();
                    }

                    checkElements = false;

                    if (purge)
                    {
                        transactionGroup.Assimilate();
                        result[elementId] = name;
                    }
                    else
                    {
                        transactionGroup.RollBack();
                    }
                }
            }

            application.DocumentChanged -= Application_DocumentChanged;

            Initialize();

            return result;
        }

        public Dictionary<ElementId, string>Purge(params string[] names)
        {
            if(names == null || document == null)
            {
                return null;
            }

            List<Element> elements = new FilteredElementCollector(document).OfClass(typeof(Material)).ToList();
            if(elements == null)
            {
                return null;
            }

            List<ElementId> elementIds = new List<ElementId>();
            foreach(Element element in elements)
            {
                if(names.Contains(element.Name))
                {
                    elementIds.Add(element.Id);
                }
            }

            return Purge(elementIds.ToArray());
        }

        private void Application_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            purge = false;

            if (!checkElements)
            {
                return;
            }

            ICollection<ElementId> elementIds_Deleted = e.GetDeletedElementIds();
            ICollection<ElementId> elementIds_Modified = e.GetModifiedElementIds();

            int count = elementIds_Deleted.Count + elementIds_Modified.Count;

            purge = count == 1;
        }
    }
}
