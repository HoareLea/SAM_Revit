using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool EarseSchema(this Document document, SAMSchema sAMSchema, bool useTransaction = true)
        {
            if (sAMSchema == null || document == null )
                return false;

            return EarseSchema(document, sAMSchema.Guid, useTransaction);
        }

        public static bool EarseSchema(this Document document, System.Guid guid, bool useTransaction = true)
        {
            if (guid == System.Guid.Empty)
                return false;
            
            Schema aSchema = Schema.Lookup(guid);
            if (aSchema == null)
                return false;

            Transaction transaction = null;

            if (useTransaction)
            {
                transaction = new Transaction(document, "Earse Schema");
                transaction.Start();
            }

            Schema.EraseSchemaAndAllEntities(aSchema, true);

            if (transaction != null)
                transaction.Commit();

            return true;
        }
    }
}