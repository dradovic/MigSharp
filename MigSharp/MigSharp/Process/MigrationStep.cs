using System.Data;
using System.Data.Common;
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

        public MigrationStep(IMigration migration, ConnectionInfo connectionInfo, IProviderFactory providerFactory)
        {
            _migration = migration;
            _connectionInfo = connectionInfo;
            _providerFactory = providerFactory;
        }

        public void Execute(IDbVersion dbVersion)
        {
            using (IDbConnection connection = OpenConnection())
            {
                Debug.Assert(connection.State == ConnectionState.Open);

                using (connection.BeginTransaction())
                {
                    Execute(connection);
                    dbVersion.Update(connection, _migration);
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

        private IDbConnection OpenConnection()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(_connectionInfo.ProviderInvariantName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = _connectionInfo.ConnectionString;
            connection.Open();
            return connection;
        }
    }
}