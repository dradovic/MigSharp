using System;
using System.Data;
using System.Data.Common;

namespace MigSharp.Process
{
    internal class SingleDbConnectionConnectionFactory : IDbConnectionFactory
    {
        private readonly IDbConnection _connection;

        public SingleDbConnectionConnectionFactory(IDbConnection connection)
        {
            _connection = new DbConnectionWrapper(connection);
        }

        public IDbConnection OpenConnection(ConnectionInfo connectionInfo)
        {
            return _connection;
        }

        public DbProviderFactory GetDbProviderFactory(ConnectionInfo connectionInfo)
        {
            throw new NotImplementedException();
        }
    }

    internal class DbConnectionWrapper : IDbConnection
    {
        private readonly IDbConnection _wrappedConnection;

        public DbConnectionWrapper(IDbConnection connection)
        {
            _wrappedConnection = connection;
        }

        public void Dispose()
        {
        }

        public IDbTransaction BeginTransaction()
        {
            return _wrappedConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _wrappedConnection.BeginTransaction(il);
        }

        public void Close()
        {
        }

        public void ChangeDatabase(string databaseName)
        {
        }

        public IDbCommand CreateCommand()
        {
            return _wrappedConnection.CreateCommand();
        }

        public void Open()
        {
            if (_wrappedConnection.State != ConnectionState.Open)
                _wrappedConnection.Open();
        }

        public string ConnectionString
        {
            get { return _wrappedConnection.ConnectionString; }
            set { }
        }

        public int ConnectionTimeout
        {
            get { return _wrappedConnection.ConnectionTimeout; }
        }

        public string Database
        {
            get { return _wrappedConnection.Database; }
        }

        public ConnectionState State
        {
            get { return _wrappedConnection.State; }
        }
    }
}