using System.Collections.Generic;
using MigSharp.Process;

namespace MigSharp
{
    internal interface IMigrationImporter
    {
        void ImportAll(out IReadOnlyCollection<ImportedMigration> migrations, out IReadOnlyCollection<ImportedAggregateMigration> aggregateMigrations);
    }
}