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

        private IVersioning _customVersioning;
        private IBootstrapping _customBootstrapping;

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
        public void MigrateTo(Assembly assembly, long timestamp)
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
            return FetchMigrationsTo(assembly, long.MaxValue);
        }

        /// <summary>
        /// Retrieves all required migrations to reach <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        /// <param name="timestamp">The timestamp to migrate to.</param>
        /// <exception cref="IrreversibleMigrationException">When the migration path would require downgrading a migration which is not reversible.</exception>
        public IMigrationBatch FetchMigrationsTo(Assembly assembly, long timestamp)
        {
            // collect all migration
            DateTime start = DateTime.Now;
            List<Tuple<IMigration, IMigrationMetadata>> migrations = CollectAllMigrations(assembly);
            Log.Info(LogCategory.Performance, "Collecting migrations took {0}ms", (DateTime.Now - start).TotalMilliseconds);

            // initialize versioning component
            IVersioning versioning = InitializeVersioning(migrations.Select(t => t.Item2));

            // filter applicable migrations)
            if (migrations.Count > 0)
            {
                List<Tuple<IMigration, IMigrationMetadata>> applicableUpMigrations = new List<Tuple<IMigration, IMigrationMetadata>>(
                    from m in migrations
                    where m.Item2.Timestamp <= timestamp && !versioning.IsContained(m.Item2)
                    orderby m.Item2.Timestamp ascending
                    select m);
                List<Tuple<IMigration, IMigrationMetadata>> applicableDownMigrations = new List<Tuple<IMigration, IMigrationMetadata>>(
                    from m in migrations
                    where m.Item2.Timestamp > timestamp && versioning.IsContained(m.Item2)
                    orderby m.Item2.Timestamp descending
                    select m);
                if (applicableDownMigrations.Any(m => !(m.Item1 is IReversibleMigration)))
                {
                    throw new IrreversibleMigrationException();
                }
                int countUp = applicableUpMigrations.Count;
                int countDown = applicableDownMigrations.Count;
                Log.Info("Found {0} (up: {1}, down: {2}) applicable migration(s)", countUp + countDown, countUp, countDown);
                if (countUp + countDown > 0)
                {
                    return new MigrationBatch(
                        applicableUpMigrations.Select(l => new MigrationStep(l.Item1, l.Item2, _connectionInfo, _providerFactory, _dbConnectionFactory)).Cast<IMigrationStep>(),
                        applicableDownMigrations.Select(l => new MigrationStep(l.Item1, l.Item2, _connectionInfo, _providerFactory, _dbConnectionFactory)).Cast<IMigrationStep>(), 
                        versioning);
                }
            }
            return MigrationBatch.Empty;
        }

        private IVersioning InitializeVersioning(IEnumerable<IMigrationMetadata> existingMigrations)
        {
            IVersioning versioning;
            if (_customVersioning != null)
            {
                versioning = _customVersioning;
            }
            else
            {
                DbVersion dbVersion = DbVersion.Create(_connectionInfo, _providerFactory, _dbConnectionFactory);
                if (_customBootstrapping != null && dbVersion.IsEmpty)
                {
                    // TODO: unit test: this should only be performed if the native versioning table does not exist yet
                    var migrationsContainedInCustomVersioning = from m in existingMigrations
                                                                where _customBootstrapping.IsContained(m)
                                                                select m;
                    dbVersion.UpdateToInclude(migrationsContainedInCustomVersioning, _connectionInfo, _dbConnectionFactory);
                }
                versioning = dbVersion;
            }
            return versioning;
        }

        public void UseCustomVersioning(IVersioning customVersioning)
        {
            if (customVersioning == null) throw new ArgumentNullException("customVersioning");
            if (_customBootstrapping != null) throw new InvalidOperationException("Either use custom versioning or custom bootstrapping.");

            _customVersioning = customVersioning;
        }

        public void UseCustomBootstrapping(IBootstrapping customBootstrapping)
        {
            if (customBootstrapping == null) throw new ArgumentNullException("customBootstrapping");
            if (_customVersioning != null) throw new InvalidOperationException("Either use custom versioning or custom bootstrapping.");

            _customBootstrapping = customBootstrapping;
        }

        private static List<Tuple<IMigration, IMigrationMetadata>> CollectAllMigrations(Assembly assembly)
        {
            Log.Info("Collecting all migrations...");
            var catalog = new AssemblyCatalog(assembly);
            var container = new CompositionContainer(catalog);
            var migrationImporter = new MigrationImporter();
            container.ComposeParts(migrationImporter);
            List<Tuple<IMigration, IMigrationMetadata>> result = 
                new List<Tuple<IMigration, IMigrationMetadata>>(
                    migrationImporter.Migrations.Select(l => new Tuple<IMigration, IMigrationMetadata>(l.Value, new MigrationMetadata(l.Metadata.Tag, l.Metadata.ModuleName, l.Value.GetType().GetTimestamp()))));
            Log.Info("Found {0} migration(s) in total", result.Count);
            return result;
        }

        private class MigrationImporter
        {
// ReSharper disable UnusedAutoPropertyAccessor.Local
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [ImportMany]
            public IEnumerable<Lazy<IMigration, IMigrationExportMetadata>> Migrations { get; set; } // set by MEF
// ReSharper restore UnusedAutoPropertyAccessor.Local
        }

        private class MigrationMetadata : IMigrationMetadata
        {
            private readonly string _tag;
            private readonly string _moduleName;
            private readonly long _timestamp;

            public string Tag { get { return _tag; } }
            public string ModuleName { get { return _moduleName; } }
            public long Timestamp { get { return _timestamp; } }

            public MigrationMetadata(string tag, string moduleName, long timestamp)
            {
                _tag = tag;
                _timestamp = timestamp;
                _moduleName = moduleName;
            }
        }
    }
}