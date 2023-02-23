using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022
        public static bool EarseSchema(this Document document, SAMSchema sAMSchema, bool useTransaction = true)
        {
            if (sAMSchema == null || document == null)
                return false;

            return EarseSchema(document, sAMSchema.Guid, useTransaction);
        }

        public static bool EarseSchema(this Document document, System.Guid guid, bool useTransaction = true)
        {
            if (guid == System.Guid.Empty)
                return false;

            Schema schema = Schema.Lookup(guid);
            if (schema == null)
                return false;

            Transaction transaction = null;

            if (useTransaction)
            {
                transaction = new Transaction(document, "Earse Schema");
                transaction.Start();
            }

            Schema.EraseSchemaAndAllEntities(schema, true);

            if (transaction != null)
                transaction.Commit();

            return true;
        }

#else
    //TODO: Schema.EraseSchemaAndAllEntities is missing in Revit 2023
#endif
    }

}