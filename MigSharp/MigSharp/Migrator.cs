using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using MigSharp.Core;
using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp
{
    /// <summary>
    /// Represents the main entry point to perform migrations.
    /// </summary>
    public class Migrator
    {
        private readonly ConnectionInfo _connectionInfo;
        private readonly ProviderFactory _providerFactory = new ProviderFactory();
        private readonly DbConnectionFactory _dbConnectionFactory = new DbConnectionFactory();

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="providerInvariantName">Invariant name of a provider. <seealso cref="DbProviderFactories.GetFactory(string)"/></param>
        public Migrator(string connectionString, string providerInvariantName)
        {
            _connectionInfo = new ConnectionInfo(connectionString, providerInvariantName);
        }

        /// <summary>
        /// Executes all pending migrations found in <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to search for migrations.</param>
        public void MigrateAll(Assembly assembly)
        {
            DateTime start = DateTime.Now;
            Log.Info("Migrating all...");

            IMigrationBatch batch = FetchPendingMigrations(assembly);
            batch.Execute();

            Log.Info(LogCategory.Performance, "All migration(s) took {0}s", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// Executes all migrations required to reach <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="timestamp"></param>
        public void MigrateTo(Assembly assembly, DateTime timestamp)
        {
            DateTime start = DateTime.Now;
            Log.Info("Migrating to {0}...", timestamp);

            IMigrationBatch batch = FetchMigrationsTo(assembly, timestamp);
            batch.Execute();

            Log.Info(LogCategory.Performance, "Migration(s) took {0}s", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// Retrieves all pending migrations.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        public IMigrationBatch FetchPendingMigrations(Assembly assembly)
        {
            return FetchMigrationsTo(assembly, DateTime.MaxValue);
        }

        /// <summary>
        /// Retrieves all pending migrations.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        /// <param name="dbVersion">The versioning component.</param>
        public IMigrationBatch FetchPendingMigrations(Assembly assembly, IDbVersion dbVersion)
        {
            return FetchMigrationsTo(assembly, DateTime.MaxValue, dbVersion);
        }

        /// <summary>
        /// Retrieves all required migrations to reach <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        /// <param name="timestamp">The timestamp to migrate to.</param>
        public IMigrationBatch FetchMigrationsTo(Assembly assembly, DateTime timestamp)
        {
            DbVersion dbVersion = DbVersion.Create(_connectionInfo, _providerFactory, _dbConnectionFactory);
            return FetchMigrationsTo(assembly, timestamp, dbVersion);
        }

        /// <summary>
        /// Retrieves all required migrations to reach <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        /// <param name="timestamp">The timestamp to migrate to.</param>
        /// <param name="dbVersion">The versioning component.</param>
        /// <returns></returns>
        public IMigrationBatch FetchMigrationsTo(Assembly assembly, DateTime timestamp, IDbVersion dbVersion)
        {
            DateTime start = DateTime.Now;
            List<Lazy<IMigration, IMigrationMetadata>> migrations = CollectAllMigrations(assembly);
            Log.Info(LogCategory.Performance, "Collecting migrations took {0}ms", (DateTime.Now - start).TotalMilliseconds);

            if (migrations.Count > 0)
            {
                var applicableUpMigrations = from m in migrations
                                             where m.Metadata.Timestamp() <= timestamp && !dbVersion.Includes(m.Metadata)
                                             orderby m.Metadata.Timestamp() ascending
                                             select m;
                int countUp = applicableUpMigrations.Count();
                var applicableDownMigrations = from m in migrations
                                               where m.Metadata.Timestamp() > timestamp && dbVersion.Includes(m.Metadata)
                                               orderby m.Metadata.Timestamp() descending
                                               select m;
                int countDown = applicableDownMigrations.Count();
                Log.Info("Found {0} (up: {1}, down: {2}) applicable migration(s)", countUp + countDown, countUp, countDown);
                if (countUp + countDown > 0)
                {
                    return new MigrationBatch(
                        applicableUpMigrations.Select(l => new MigrationStep(l.Value, l.Metadata, _connectionInfo, _providerFactory, _dbConnectionFactory)).Cast<IMigrationStep>(),
                        applicableDownMigrations.Select(l => new MigrationStep(l.Value, l.Metadata, _connectionInfo, _providerFactory, _dbConnectionFactory)).Cast<IMigrationStep>(), 
                        dbVersion);
                }
            }
            return MigrationBatch.Empty;
        }

        private static List<Lazy<IMigration, IMigrationMetadata>> CollectAllMigrations(Assembly assembly)
        {
            Log.Info("Collecting all migrations...");
            var catalog = new AssemblyCatalog(assembly);
            var container = new CompositionContainer(catalog);
            var migrationImporter = new MigrationImporter();
            container.ComposeParts(migrationImporter);
            var result = new List<Lazy<IMigration, IMigrationMetadata>>(migrationImporter.Migrations);
            Log.Info("Found {0} migration(s) in total", result.Count);
            return result;
        }

        private class MigrationImporter
        {
// ReSharper disable UnusedAutoPropertyAccessor.Local
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [ImportMany]
            public IEnumerable<Lazy<IMigration, IMigrationMetadata>> Migrations { get; set; } // set by MEF
// ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}