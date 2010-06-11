using System.Data;
using System.Data.Common;

namespace MigSharp.Process
{
    internal class DbConnectionFactory : IDbConnectionFactory
    {
        public IDbConnection OpenConnection(ConnectionInfo connectionInfo)
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory(connectionInfo.ProviderInvariantName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionInfo.ConnectionString;
            connection.Open();
            return connection;
        }
    }
}