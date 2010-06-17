using System;
using System.Data;
using System.Diagnostics;

using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationStep
    {
        private readonly IMigration _migration;
        private readonly IMigrationMetadata _metadata;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IProviderFactory _providerFactory;
        private readonly IDbConnectionFactory _connectionFactory;

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
        /// <param name="dbVersion">Might be null in the case of a bootstrap step.</param>
        public void Execute(IDbVersion dbVersion)
        {
            using (IDbConnection connection = _connectionFactory.OpenConnection(_connectionInfo))
            {
                Debug.Assert(connection.State == ConnectionState.Open);

                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    Execute(connection, transaction);
                    if (dbVersion != null)
                    {
                        dbVersion.Update(_metadata, connection, transaction);
                    }
                    transaction.Commit();
                }
            }
        }

        private void Execute(IDbConnection connection, IDbTransaction transaction)
        {
            Debug.Assert(connection.State == ConnectionState.Open);

            Database database = new Database();
            _migration.Up(database);
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