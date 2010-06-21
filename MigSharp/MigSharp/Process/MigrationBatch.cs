using System;
using System.Collections.Generic;
using System.Globalization;

using MigSharp.Core;
using MigSharp.Providers;

using System.Linq;

namespace MigSharp.Process
{
    internal class MigrationBatch : IMigrationBatch
    {
        public static readonly IMigrationBatch Empty = new EmptyMigrationBatch();

        private readonly IEnumerable<Lazy<IMigration, IMigrationMetadata>> _upMigrations;
        private readonly IEnumerable<Lazy<IMigration, IMigrationMetadata>> _downMigrations;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IProviderFactory _providerFactory;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbVersion _dbVersion;

        // TODO: unit test events
        public event EventHandler<MigrationEventArgs> MigrationFinished;
        //public event EventHandler<CancelableMigrationEventArgs> MigrationStarting;

        public int Count { get { return _upMigrations.Count() + _downMigrations.Count(); } }

        public MigrationBatch(
            IEnumerable<Lazy<IMigration, IMigrationMetadata>> upMigrations, 
            IEnumerable<Lazy<IMigration, IMigrationMetadata>> downMigrations, 
            ConnectionInfo connectionInfo, 
            IProviderFactory providerFactory, 
            IDbConnectionFactory connectionFactory, 
            IDbVersion dbVersion)
        {
            _upMigrations = upMigrations;
            _downMigrations = downMigrations;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
            _connectionFactory = connectionFactory;
            _dbVersion = dbVersion;
        }

        public void Execute()
        {
            foreach (Lazy<IMigration, IMigrationMetadata> migration in _downMigrations)
            {
                ExecuteStep(migration.Value, migration.Metadata, _dbVersion, MigrationDirection.Down);
            }
            foreach (Lazy<IMigration, IMigrationMetadata> migration in _upMigrations)
            {
                ExecuteStep(migration.Value, migration.Metadata, _dbVersion, MigrationDirection.Up);
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

        internal class EmptyMigrationBatch : IMigrationBatch
        {
            public event EventHandler<MigrationEventArgs> MigrationFinished;
            //public event EventHandler<CancelableMigrationEventArgs> MigrationStarting;

            public int Count { get { return 0; } }

            public void Execute()
            {
            }
        }
    }
}