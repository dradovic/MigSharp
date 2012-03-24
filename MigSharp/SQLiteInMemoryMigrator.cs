using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using MigSharp.Core;
using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp
{
    public class SqliteInMemoryMigrator : Migrator
    {
        public SqliteInMemoryMigrator(IDbConnection connection)
            : base(string.Empty, ProviderNames.SQLite)
        {
            base._dbConnectionFactory = new SqliteInMemoryDbConnectionFactory(connection);
        }

        internal class SqliteInMemoryDbConnectionFactory : IDbConnectionFactory
        {
            private readonly IDbConnection _connection;

            internal SqliteInMemoryDbConnectionFactory(IDbConnection connection)
            {
                _connection = new SqliteInMemoryDbConnectionWrapper(connection);
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

        internal class SqliteInMemoryDbConnectionWrapper : IDbConnection
        {
            private readonly IDbConnection _wrappedConnection;

            public SqliteInMemoryDbConnectionWrapper(IDbConnection connection)
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
}