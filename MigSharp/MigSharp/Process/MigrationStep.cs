using System.Data;
using System.Diagnostics;

using MigSharp.Core;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationStep
    {
        private readonly IMigration _migration;
        private readonly ConnectionInfo _connectionInfo;
        private readonly IProviderFactory _providerFactory;
        private readonly IDbConnectionFactory _connectionFactory;

        public MigrationStep(IMigration migration, ConnectionInfo connectionInfo, IProviderFactory providerFactory, IDbConnectionFactory connectionFactory)
        {
            _migration = migration;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
            _connectionFactory = connectionFactory;
        }

        public void Execute(IDbVersion dbVersion)
        {
            using (IDbConnection connection = _connectionFactory.OpenConnection(_connectionInfo))
            {
                Debug.Assert(connection.State == ConnectionState.Open);

                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    Execute(connection);
                    dbVersion.Update(connection, _migration);
                    transaction.Commit();
                }
            }
        }

        private void Execute(IDbConnection connection)
        {
            Debug.Assert(connection.State == ConnectionState.Open);

            Database database = new Database();
            _migration.Up(database);
            IProvider provider = _providerFactory.GetProvider(_connectionInfo.ProviderInvariantName);
            CommandScripter scripter = new CommandScripter(provider);
            foreach (string commandText in scripter.GetCommandTexts(database))
            {
                IDbCommand command = connection.CreateCommand();
                command.CommandText = commandText;
                command.ExecuteNonQuery(); // TODO: add logging
            }
        }
    }
}