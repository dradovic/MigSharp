using System.Data;
using System.Data.Common;

namespace MigSharp.Process
{
    internal class DbConnectionFactory : IDbConnectionFactory
    {
        public IDbConnection OpenConnection(ConnectionInfo connectionInfo)
        {
            DbProviderFactory factory = GetDbProviderFactory(connectionInfo);
            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionInfo.ConnectionString;
            connection.Open();
            return connection;
        }

        public DbProviderFactory GetDbProviderFactory(ConnectionInfo connectionInfo)
        {
            return DbProviderFactories.GetFactory(connectionInfo.ProviderInvariantName);
        }
    }
}