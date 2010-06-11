using System;
using System.Collections.Generic;
using System.Linq;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationBatch
    {
        private readonly IEnumerable<Lazy<IMigration, IMigrationMetaData>> _migrations;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IProviderFactory _providerFactory;

        public MigrationBatch(IEnumerable<Lazy<IMigration, IMigrationMetaData>> migrations, ConnectionInfo connectionInfo, IProviderFactory providerFactory)
        {
            _migrations = migrations;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
        }

        public void Process(IDbVersion dbVersion)
        {
            var applicableMigrations = from m in _migrations
                                       where !dbVersion.Includes(m.Metadata)
                                       orderby m.Metadata.Timestamp
                                       select m.Value;
            foreach (IMigration migration in applicableMigrations)
            {
                var step = new MigrationStep(migration, _connectionInfo, _providerFactory);
                step.Execute(dbVersion);
            }
        }
    }
}