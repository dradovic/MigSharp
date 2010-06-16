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
        private readonly IProviderFactory _providerFactory; // TODO: extract as a service and get via service location
        private readonly IDbConnectionFactory _connectionFactory; // TODO: extract as a service and get via service location

        public MigrationBatch(IEnumerable<Lazy<IMigration, IMigrationMetaData>> migrations, ConnectionInfo connectionInfo, IProviderFactory providerFactory, IDbConnectionFactory connectionFactory)
        {
            _migrations = migrations;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
            _connectionFactory = connectionFactory;
        }

        public void Process(IDbVersion dbVersion)
        {
            var applicableMigrations = from m in _migrations
                                       where !dbVersion.Includes(m.Metadata)
                                       orderby m.Metadata.Timestamp()
                                       select new { m.Value, m.Metadata };
            foreach (var m in applicableMigrations)
            {
                var step = new MigrationStep(m.Value, m.Metadata, _connectionInfo, _providerFactory, _connectionFactory);
                step.Execute(dbVersion);
            }
        }
    }
}