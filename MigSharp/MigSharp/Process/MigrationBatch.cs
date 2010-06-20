using System;
using System.Collections.Generic;
using System.Globalization;

using MigSharp.Core;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationBatch
    {
        private readonly IEnumerable<Lazy<IMigration, IMigrationMetadata>> _upMigrations;
        private readonly IEnumerable<Lazy<IMigration, IMigrationMetadata>> _downMigrations;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IProviderFactory _providerFactory;
        private readonly IDbConnectionFactory _connectionFactory;

        public MigrationBatch(
            IEnumerable<Lazy<IMigration, IMigrationMetadata>> upMigrations,
            IEnumerable<Lazy<IMigration, IMigrationMetadata>> downMigrations, 
            ConnectionInfo connectionInfo, 
            IProviderFactory providerFactory, 
            IDbConnectionFactory connectionFactory)
        {
            _upMigrations = upMigrations;
            _downMigrations = downMigrations;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
            _connectionFactory = connectionFactory;
        }

        public void Execute(IDbVersion dbVersion)
        {
            foreach (Lazy<IMigration, IMigrationMetadata> migration in _downMigrations)
            {
                ExecuteStep(migration.Value, migration.Metadata, dbVersion, MigrationDirection.Down);
            }
            foreach (Lazy<IMigration, IMigrationMetadata> migration in _upMigrations)
            {
                ExecuteStep(migration.Value, migration.Metadata, dbVersion, MigrationDirection.Up);
            }
        }

        private void ExecuteStep(IMigration migration, IMigrationMetadata metadata, IDbVersion dbVersion, MigrationDirection direction)
        {
            DateTime start = DateTime.Now;

            var step = new MigrationStep(migration, metadata, _connectionInfo, _providerFactory, _connectionFactory);
            step.Execute(dbVersion, direction);

            Log.Info(LogCategory.Performance, "Migration to {0}{1}{2} took {3}s",
                metadata.Timestamp(),
                !string.IsNullOrEmpty(metadata.ModuleName) ? string.Format(CultureInfo.CurrentCulture, " [{0}]", metadata.ModuleName) : string.Empty,
                !string.IsNullOrEmpty(metadata.Tag) ? string.Format(CultureInfo.CurrentCulture, " '{0}'", metadata.Tag) : string.Empty,
                (DateTime.Now - start).TotalSeconds);
        }
    }
}