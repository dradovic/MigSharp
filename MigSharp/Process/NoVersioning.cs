using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MigSharp.Process
{
    internal class NoVersioning : IVersioning
    {
        public IEnumerable<IMigrationMetadata> ExecutedMigrations
        {
            get { return Enumerable.Empty<IMigrationMetadata>(); }
        }

        public void Update(IScheduledMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction,
                           IDbCommandExecutor commandExecutor)
        {
            // nothing to do
        }
    }
}