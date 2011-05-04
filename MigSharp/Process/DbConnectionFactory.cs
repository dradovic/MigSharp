using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace MigSharp.Process
{
    internal class DbConnectionFactory : IDbConnectionFactory
    {
        public IDbConnection OpenConnection(ConnectionInfo connectionInfo)
        {
            DbProviderFactory factory = GetDbProviderFactory(connectionInfo);
            var connection = factory.CreateConnection();
            if (connection == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to create a connection for ADO.NET provider '{0}'.", connectionInfo.ProviderInvariantName));
            }
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