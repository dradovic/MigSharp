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
        private readonly IScheduledMigrationMetadata _metadata;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ISqlDispatcher _sqlDispatcher;

        private string MigrationName { get { return Migration.GetType().FullName; } }

        public IScheduledMigrationMetadata Metadata { get { return _metadata; } }

        public MigrationStep(IMigration migration, IScheduledMigrationMetadata metadata, ConnectionInfo connectionInfo, IProvider provider, IProviderMetadata providerMetadata, IDbConnectionFactory connectionFactory, ISqlDispatcher sqlDispatcher)
            : base(migration, provider, providerMetadata)
        {
            _metadata = metadata;
            _connectionInfo = connectionInfo;
            _connectionFactory = connectionFactory;
            _sqlDispatcher = sqlDispatcher;
        }

        public IMigrationReport Report(IMigrationContext context)
        {
            Database database = GetDatabaseContainingMigrationChanges(_metadata.Direction, context);
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
                    IDbCommandExecutor executor;
                    using ((executor = _sqlDispatcher.CreateExecutor(string.Format(CultureInfo.InvariantCulture, "Migration.{0}.{1}", _metadata.ModuleName, _metadata.Timestamp))) as IDisposable)
                    {
                        try
                        {
                            Execute(connection, transaction, _metadata.Direction, executor);
                        }
                        catch
                        {
                            Log.Error("An non-recoverable error occurred in Migration '{0}'{1}{2} while executing {3}.", 
                                _metadata.Timestamp,
                                _metadata.ModuleName != MigrationExportAttribute.DefaultModuleName ? " in module '" + _metadata.ModuleName + "'" : string.Empty,
                                !string.IsNullOrEmpty(_metadata.Tag) ? ": '" + _metadata.Tag + "'" : string.Empty,
                                _metadata.Direction);
                            throw;
                        }

                        // update versioning
                        versioning.Update(_metadata, connection, transaction, executor);
                    }

                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                }
            }

            Log.Verbose(LogCategory.Performance, "Migration of module '{0}' to {1}{2} took {3}s",
                _metadata.ModuleName,
                _metadata.Timestamp,
                !string.IsNullOrEmpty(_metadata.Tag) ? string.Format(CultureInfo.CurrentCulture, " '{0}'", _metadata.Tag) : string.Empty,
                (DateTime.Now - start).TotalSeconds);
        }
    }
}