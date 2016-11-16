using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using MigSharp.Core;

namespace MigSharp.Process
{
    /// <summary>
    /// Implements <see cref="IVersioning"/> without creating a versioning table until it is really needed.
    /// </summary>
    internal class Versioning : IVersioning
    {
        private readonly IRuntimeConfiguration _configuration;
        private readonly TableName _versioningTableName;

        private PersistedVersioning _persistedVersioning;
        private Lazy<bool> _versioningTableExists;

        internal bool VersioningTableExists { get { return _versioningTableExists.Value; } }

        internal Versioning(IRuntimeConfiguration configuration, TableName versioningTableName)
        {
            _configuration = configuration;
            _versioningTableName = versioningTableName;

            _versioningTableExists = new Lazy<bool>(() =>
            {
                int exists;
                using (IDbConnection connection = configuration.OpenConnection())
                {
                    IDbCommand command = connection.CreateCommand();
                    command.CommandTimeout = 0; // do not timeout; the client is responsible for not causing lock-outs
                    command.CommandText = configuration.ProviderInfo.Provider.ExistsTable(connection.Database, _versioningTableName);
                    Log.Verbose(LogCategory.Sql, command.CommandText);
                    exists = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                }
                return exists != 0;
            });
        }

        internal void UpdateToInclude(IEnumerable<IMigrationMetadata> containedMigrations, IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommandExecutor executor;
            using ((executor = _configuration.SqlDispatcher.CreateExecutor("CustomBootstrapping")) as IDisposable)
            {
                PersistedVersioning versioning = GetPersistedVersioning(connection, transaction, executor);
                versioning.UpdateToInclude(containedMigrations, connection, transaction, executor);
            }
        }

        private PersistedVersioning GetPersistedVersioning(IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor executor)
        {
            if (_persistedVersioning == null)
            {
                var history = new History(_versioningTableName, _configuration.ProviderInfo.Metadata);
                if (!_versioningTableExists.Value)
                {
                    Debug.Assert(connection != null, "At this point, an upgrade of the versioning table is requested. This always takes part of a running migration step and therefore already has an associated connection (and possibly a transaction).");

                    // execute the boostrap migration to create the versioning table
                    var step = new BootstrapMigrationStep(new BootstrapMigration(_versioningTableName), null);
                    step.Execute(_configuration.ProviderInfo, connection, transaction, MigrationDirection.Up, executor);
                    _versioningTableExists = new Lazy<bool>(() => true); // now, the versioning table exists
                }
                else
                {
                    // load the existing entries from the versioning table
                    IDbConnection c = connection ?? _configuration.OpenConnection();
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

        public IEnumerable<IMigrationMetadata> ExecutedMigrations
        {
            get
            {
                if (!_versioningTableExists.Value)
                {
                    return Enumerable.Empty<IMigrationMetadata>();
                }
                PersistedVersioning versioning = GetPersistedVersioning(null, null, null);
                return versioning.ExecutedMigrations;
            }
        }

        public void Update(IMigrationStepMetadata metadata, IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor commandExecutor)
        {
            PersistedVersioning versioning = GetPersistedVersioning(connection, transaction, commandExecutor);
            versioning.Update(metadata, connection, transaction, commandExecutor);
        }

        #endregion
    }
}