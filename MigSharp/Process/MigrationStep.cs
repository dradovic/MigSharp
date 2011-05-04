using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationStep : BootstrapMigrationStep, IMigrationStep
    {
        private readonly IMigrationMetadata _metadata;
        private readonly MigrationDirection _direction;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IDbConnectionFactory _connectionFactory;

        private string MigrationName { get { return Migration.GetType().FullName; } }

        public IMigrationMetadata Metadata { get { return _metadata; } }

        public MigrationDirection Direction { get { return _direction; } }

        public MigrationStep(IMigration migration, IMigrationMetadata metadata, MigrationDirection direction, ConnectionInfo connectionInfo, IProvider provider, IProviderMetadata providerMetadata, IDbConnectionFactory connectionFactory)
            : base(migration, provider, providerMetadata)
        {
            _metadata = metadata;
            _direction = direction;
            _connectionInfo = connectionInfo;
            _connectionFactory = connectionFactory;
        }

        public IMigrationReport Report(IMigrationContext context)
        {
            Database database = GetDatabaseContainingMigrationChanges(_direction, context);
            return MigrationReport.Create(database, MigrationName);
        }

        /// <summary>
        /// Executes the migration step and updates the versioning information in one transaction.
        /// </summary>
        public void Execute(IVersioning versioning)
        {
            if (versioning == null) throw new ArgumentNullException("versioning");

            DateTime start = DateTime.Now;

            using (IDbConnection connection = _connectionFactory.OpenConnection(_connectionInfo))
            {
                Debug.Assert(connection.State == ConnectionState.Open);

                using (IDbTransaction transaction = _connectionInfo.SupportsTransactions ? connection.BeginTransaction() : null)
                {
                    Execute(connection, transaction, _direction);

                    // update versioning
                    versioning.Update(_metadata, connection, transaction, _direction);
                    Debug.Assert(versioning.IsContained(_metadata) == (_direction == MigrationDirection.Up), "The post-condition of IVersioning.Update is violated.");

                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                }
            }

            Log.Verbose(LogCategory.Performance, "Migration to {0}{1}{2} took {3}s",
                _metadata.Timestamp,
                !string.IsNullOrEmpty(_metadata.ModuleName) ? string.Format(CultureInfo.CurrentCulture, " [{0}]", _metadata.ModuleName) : string.Empty,
                !string.IsNullOrEmpty(_metadata.Tag) ? string.Format(CultureInfo.CurrentCulture, " '{0}'", _metadata.Tag) : string.Empty,
                (DateTime.Now - start).TotalSeconds);
        }
    }
}