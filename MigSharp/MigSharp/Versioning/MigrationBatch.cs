using System;
using System.Collections.Generic;
using System.Linq;

namespace MigSharp.Versioning
{
    internal class MigrationBatch
    {
        private readonly IEnumerable<Lazy<IMigration, IMigrationMetaData>> _migrations;

        public MigrationBatch(IEnumerable<Lazy<IMigration, IMigrationMetaData>> migrations)
        {
            _migrations = migrations;
        }

        public void Process(IDbVersion dbVersion, string connectionString)
        {
            var applicableMigrations = from m in _migrations
                                       where !dbVersion.Includes(m.Metadata)
                                       orderby m.Metadata.Timestamp
                                       select m.Value;
            foreach (IMigration migration in applicableMigrations)
            {
                var step = new MigrationStep(migration);
                step.Execute(dbVersion, connectionString);
            }
        }
    }
}