using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MigSharp.Process
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "It is clearly stated in the documentation that the caller is responsible of disposing the connection (if a custom connection is used).")]  
    internal class DbConnectionFactory : IDbConnectionFactory
    {
        private IDbConnection _connection;

        public IDbConnection OpenConnection(ConnectionInfo connectionInfo)
        {
            if (_connection == null)
            {
                DbProviderFactory factory = GetDbProviderFactory(connectionInfo);
                var connection = factory.CreateConnection();
                if (connection == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                                                      "Unable to create a connection for ADO.NET provider '{0}'.",
                                                                      connectionInfo.ProviderInvariantName));
                }
                connection.ConnectionString = connectionInfo.ConnectionString;
                connection.Open();

                // enable ANSI quoting if needed
                if (!string.IsNullOrEmpty(connectionInfo.EnableAnsiQuotesCommand))
                {
                    DbCommand command = connection.CreateCommand();
                    command.CommandText = connectionInfo.EnableAnsiQuotesCommand;
                    command.ExecuteNonQuery();
                }

                return connection;
            }
            else
            {
                return _connection;
            }
        }

        public DbProviderFactory GetDbProviderFactory(ConnectionInfo connectionInfo)
        {
            return DbProviderFactories.GetFactory(connectionInfo.ProviderInvariantName);
        }

        public void UseCustomConnection(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _connection = new DbConnectionWrapper(connection);
        }
    }
}