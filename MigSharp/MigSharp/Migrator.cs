using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

using MigSharp.Core;
using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp
{
    public class Migrator
    {
        private readonly ConnectionInfo _connectionInfo;

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="providerInvariantName">Invariant name of a provider. <seealso cref="DbProviderFactories.GetFactory(string)"/></param>
        public Migrator(string connectionString, string providerInvariantName)
        {
            _connectionInfo = new ConnectionInfo(connectionString, providerInvariantName);
        }

        public void UpgradeAll(Assembly assembly)
        {
            Log.Info("Upgrading all...");
            Updgrade(assembly, DateTime.MaxValue);
        }

        public void UpgradeUntil(Assembly assembly, DateTime timestamp)
        {
            Log.Info(string.Format(CultureInfo.CurrentCulture, "Upgrading until {0}...", timestamp));
            Updgrade(assembly, timestamp);
        }

        private void Updgrade(Assembly assembly, DateTime timestamp)
        {
            List<Lazy<IMigration, IMigrationMetaData>> migrations = CollectMigrations(assembly);
            if (migrations.Count > 0)
            {
                var providerFactory = new ProviderFactory();
                var connectionFactory = new DbConnectionFactory();
                var batch = new MigrationBatch(migrations, timestamp, _connectionInfo, providerFactory, connectionFactory);
                var dbVersion = DbVersion.Create(_connectionInfo, providerFactory, connectionFactory);
                batch.Process(dbVersion);
            }
        }

        private static List<Lazy<IMigration, IMigrationMetaData>> CollectMigrations(Assembly assembly)
        {
            Log.Info("Collecting all migrations...");
            var catalog = new AssemblyCatalog(assembly);
            var container = new CompositionContainer(catalog);
            var migrationImporter = new MigrationImporter();
            container.ComposeParts(migrationImporter);
            var result = new List<Lazy<IMigration, IMigrationMetaData>>(migrationImporter.Migrations);
            Log.Info("Found {0} migration(s)", result.Count);
            return result;
        }

        private class MigrationImporter
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [ImportMany]
// ReSharper disable UnusedAutoPropertyAccessor.Local
            public IEnumerable<Lazy<IMigration, IMigrationMetaData>> Migrations { get; set; } // set by MEF
// ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}