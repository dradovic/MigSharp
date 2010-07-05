using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationStep : IMigrationStep
    {
        private readonly IMigration _migration;
        private readonly IMigrationMetadata _metadata;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IProviderFactory _providerFactory;
        private readonly IDbConnectionFactory _connectionFactory;

        public IMigrationMetadata Metadata { get { return _metadata; } }

        public MigrationStep(IMigration migration, IMigrationMetadata metadata, ConnectionInfo connectionInfo, IProviderFactory providerFactory, IDbConnectionFactory connectionFactory)
        {
            _migration = migration;
            _metadata = metadata;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Executes the migration step and updates the versioning information in one transaction.
        /// </summary>
        /// <param name="versioning">Might be null in the case of a bootstrap step.</param>
        /// <param name="direction"></param>
        public void Execute(IVersioning versioning, MigrationDirection direction)
        {
            DateTime start = DateTime.Now;

            using (IDbConnection connection = _connectionFactory.OpenConnection(_connectionInfo))
            {
                Debug.Assert(connection.State == ConnectionState.Open);

                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    Execute(connection, transaction, direction);
                    if (versioning != null)
                    {
                        versioning.Update(_metadata, connection, transaction, direction);
                        Debug.Assert(versioning.IsContained(_metadata) == (direction == MigrationDirection.Up), "The post-condition of IVersioning.Update is violated.");
                    }
                    transaction.Commit();
                }
            }

            Log.Info(LogCategory.Performance, "Migration to {0}{1}{2} took {3}s",
                _metadata.Timestamp,
                !string.IsNullOrEmpty(_metadata.ModuleName) ? string.Format(CultureInfo.CurrentCulture, " [{0}]", _metadata.ModuleName) : string.Empty,
                !string.IsNullOrEmpty(_metadata.Tag) ? string.Format(CultureInfo.CurrentCulture, " '{0}'", _metadata.Tag) : string.Empty,
                (DateTime.Now - start).TotalSeconds);
        }

        private void Execute(IDbConnection connection, IDbTransaction transaction, MigrationDirection direction)
        {
            Debug.Assert(connection.State == ConnectionState.Open);

            Database database = new Database(new MigrationContext(connection, transaction));
            if (direction == MigrationDirection.Up)
            {
                _migration.Up(database);
            }
            else
            {
                Debug.Assert(direction == MigrationDirection.Down);
                IReversibleMigration migration = _migration as IReversibleMigration;
                if (migration == null)
                {
                    throw new InvalidOperationException("Cannot downgrade an irreversible migration."); // this should never happen
                }
                migration.Down(database);
            }
            IProviderMetadata metadata;
            IProvider provider = _providerFactory.GetProvider(_connectionInfo.ProviderInvariantName, out metadata);
            CommandScripter scripter = new CommandScripter(provider, metadata);
            foreach (string commandText in scripter.GetCommandTexts(database))
            {
                Log.Info(LogCategory.Sql, commandText); // TODO: this should be only logged in a verbose mode

                IDbCommand command = connection.CreateCommand();
                command.CommandTimeout = 0; // do not timeout; the client is responsible for not causing lock-outs
                command.Transaction = transaction;
                command.CommandText = commandText;
                command.ExecuteNonQuery();
            }
        }
    }
}