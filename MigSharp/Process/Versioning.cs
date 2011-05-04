using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Core;
using MigSharp.Providers;

namespace MigSharp.Process
{
    /// <summary>
    /// Implements <see cref="IVersioning"/> without creating a versioning table until it is really needed.
    /// </summary>
    internal class Versioning : IVersioning
    {
        private readonly ConnectionInfo _connectionInfo;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IProvider _provider;
        private readonly IProviderMetadata _providerMetadata;
        private readonly string _versioningTableName;

        private PersistedVersioning _persistedVersioning;
        private Lazy<bool> _versioningTableExists;

        internal bool VersioningTableExists { get { return _versioningTableExists.Value; } }

        internal Versioning(ConnectionInfo connectionInfo, IDbConnectionFactory connectionFactory, IProvider provider, IProviderMetadata providerMetadata, string versioningTableName)
        {
            _connectionInfo = connectionInfo;
            _connectionFactory = connectionFactory;
            _provider = provider;
            _providerMetadata = providerMetadata;
            _versioningTableName = versioningTableName;

            _versioningTableExists = new Lazy<bool>(() =>
                {
                    int exists;
                    using (IDbConnection connection = connectionFactory.OpenConnection(connectionInfo))
                    {
                        IDbCommand command = connection.CreateCommand();
                        command.CommandTimeout = 0; // do not timeout; the client is responsible for not causing lock-outs
                        command.CommandText = provider.ExistsTable(connection.Database, _versioningTableName);
                        Log.Verbose(LogCategory.Sql, command.CommandText);
                        exists = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                    }
                    return exists != 0;
                });
        }

        internal void UpdateToInclude(IEnumerable<IMigrationMetadata> containedMigrations, IDbConnection connection, IDbTransaction transaction)
        {
            PersistedVersioning versioning = GetPersistedVersioning(connection, transaction);
            versioning.UpdateToInclude(containedMigrations, connection, transaction);
        }

        private PersistedVersioning GetPersistedVersioning(IDbConnection connection, IDbTransaction transaction)
        {
            if (_persistedVersioning == null)
            {
                var history = new History(_versioningTableName, _providerMetadata);
                if (!_versioningTableExists.Value)
                {
                    Debug.Assert(connection != null, "At this point, an upgrade of the versioning table is requested. This always takes part of a running migration step and therefore already has an associated connection (and possibly a transaction).");

                    // execute the boostrap migration to create the versioning table
                    var step = new BootstrapMigrationStep(new BootstrapMigration(_versioningTableName), _provider, _providerMetadata);
                    step.Execute(connection, transaction, MigrationDirection.Up);
                    _versioningTableExists = new Lazy<bool>(() => true); // now, the versioning table exists
                }
                else
                {
                    // load the existing entries from the versioning table
                    IDbConnection c = connection ?? _connectionFactory.OpenConnection(_connectionInfo);
                    try
                    {
                        history.Load(c, transaction);
                    }
                    finally
                    {
                        if (connection == null) // we had to open a connection ourselves
                        {
                            c.Dispose();
                        }
                    }
                }
                _persistedVersioning = new PersistedVersioning(history);
            }
            Debug.Assert(_persistedVersioning != null);
            return _persistedVersioning;
        }

        #region Implementation of IVersioning

        public bool IsContained(IMigrationMetadata metadata)
        {
            if (!_versioningTableExists.Value)
            {
                return false;
            }
            PersistedVersioning versioning = GetPersistedVersioning(null, null);
            return versioning.IsContained(metadata);
        }

        public void Update(IMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction, MigrationDirection direction)
        {
            PersistedVersioning versioning = GetPersistedVersioning(connection, transaction);
            versioning.Update(metadata, connection, transaction, direction);
        }

        #endregion
    }
}