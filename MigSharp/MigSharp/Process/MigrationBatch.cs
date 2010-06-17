using System;
using System.Collections.Generic;

using MigSharp.Core;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationBatch
    {
        private readonly IEnumerable<Lazy<IMigration, IMigrationMetaData>> _migrations;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IProviderFactory _providerFactory;
        private readonly IDbConnectionFactory _connectionFactory;

        public MigrationBatch(
            IEnumerable<Lazy<IMigration, IMigrationMetaData>> migrations, 
            ConnectionInfo connectionInfo, 
            IProviderFactory providerFactory, 
            IDbConnectionFactory connectionFactory)
        {
            _migrations = migrations;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
            _connectionFactory = connectionFactory;
        }

        public void Execute(IDbVersion dbVersion)
        {
            foreach (var m in _migrations)
            {
                DateTime start = DateTime.Now;

                var step = new MigrationStep(m.Value, m.Metadata, _connectionInfo, _providerFactory, _connectionFactory);
                step.Execute(dbVersion);

                Log.Info(LogCategory.Performance, "Migration to {0}{1}{2} took {3}s",
                    m.Metadata.Timestamp(),
                    !string.IsNullOrEmpty(m.Metadata.Module) ? string.Format(" [{0}]", m.Metadata.Module) : string.Empty,
                    !string.IsNullOrEmpty(m.Metadata.Tag) ? string.Format(" '{0}'", m.Metadata.Tag) : string.Empty,
                    (DateTime.Now - start).TotalSeconds);
            }
        }
    }
}