using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data;
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
        private readonly IProvider _provider;
        private readonly IProviderMetadata _providerMetadata;
        private readonly DbConnectionFactory _dbConnectionFactory = new DbConnectionFactory();
        private readonly MigrationOptions _options;

        private IVersioning _customVersioning;
        private IBootstrapper _customBootstrapper;

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="providerName">The name of the provider that should be used for this migrator (<see cref="ProviderNames"/>).</param>
        /// <param name="options">Options.</param>
        public Migrator(string connectionString, string providerName, MigrationOptions options)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (providerName == null) throw new ArgumentNullException("providerName");
            if (options == null) throw new ArgumentNullException("options");

            _provider = options.SupportedProviders.GetProvider(providerName, out _providerMetadata);

            _connectionInfo = new ConnectionInfo(connectionString, _providerMetadata.InvariantName, _providerMetadata.SupportsTransactions);
            _options = options;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/> for a specific module.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="providerName">The name of the provider that should be used for this migrator (<see cref="ProviderNames"/>).</param>
        /// <param name="moduleName">The name of the module whose migrations should be executed.</param>
        public Migrator(string connectionString, string providerName, string moduleName) :
            this(connectionString, providerName, new MigrationOptions(moduleName))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/> with default options.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="providerName">The name of the provider that should be used for this migrator (<see cref="ProviderNames"/>).</param>
        public Migrator(string connectionString, string providerName) : // signature used in a Wiki example
            this(connectionString, providerName, new MigrationOptions())
        {
        }

        /// <summary>
        /// Executes all pending migrations found in <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly to search for migrations.</param>
        public void MigrateAll(Assembly assembly) // signature used in a Wiki example
        {
            DateTime start = DateTime.Now;
            Log.Info("Migrating all...");

            IMigrationBatch batch = FetchMigrations(assembly);
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

            Log.Info(LogCategory.Performance, "Migration(s) to {0} took {1}s", timestamp, (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// Retrieves all pending migrations.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        public IMigrationBatch FetchMigrations(Assembly assembly)
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
            // collect all migrations
            DateTime start = DateTime.Now;
            List<MigrationInfo> migrations = CollectAllMigrationsForModule(assembly, _options.ModuleSelector);
            Log.Verbose(LogCategory.Performance, "Collecting migrations took {0}s", (DateTime.Now - start).TotalSeconds);

            // initialize versioning component
            IVersioning versioning = InitializeVersioning(assembly);

            // filter applicable migrations)
            if (migrations.Count > 0)
            {
                List<MigrationInfo> applicableUpMigrations = new List<MigrationInfo>(
                    from m in migrations
                    where m.Metadata.Timestamp <= timestamp && !versioning.IsContained(m.Metadata)
                    orderby m.Metadata.Timestamp ascending
                    select m);
                List<MigrationInfo> applicableDownMigrations = new List<MigrationInfo>(
                    from m in migrations
                    where m.Metadata.Timestamp > timestamp && versioning.IsContained(m.Metadata)
                    orderby m.Metadata.Timestamp descending
                    select m);
                if (applicableDownMigrations.Any(m => !(m.Migration is IReversibleMigration)))
                {
                    throw new IrreversibleMigrationException();
                }
                int countUp = applicableUpMigrations.Count;
                int countDown = applicableDownMigrations.Count;
                Log.Info("Found {0} (up: {1}, down: {2}) applicable migration(s)", countUp + countDown, countUp, countDown);
                if (countUp + countDown > 0)
                {
                    return new MigrationBatch(
// ReSharper disable RedundantEnumerableCastCall
                        applicableUpMigrations.Select(l => new MigrationStep(l.Migration, l.Metadata, MigrationDirection.Up, _connectionInfo, _provider, _providerMetadata, _dbConnectionFactory)).Cast<IMigrationStep>(),
                        applicableDownMigrations.Select(l => new MigrationStep(l.Migration, l.Metadata, MigrationDirection.Down, _connectionInfo, _provider, _providerMetadata, _dbConnectionFactory)).Cast<IMigrationStep>(),
// ReSharper restore RedundantEnumerableCastCall
                        versioning,
                        _options);
                }
            }
            return MigrationBatch.Empty;
        }

        /// <summary>
        /// Checks if any migrations are pending to be performed.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        public bool IsUpToDate(Assembly assembly)
        {
            return FetchMigrations(assembly).Count == 0;
        }

        private IVersioning InitializeVersioning(Assembly assembly)
        {
            IVersioning versioning;
            if (_customVersioning != null)
            {
                versioning = _customVersioning;
            }
            else
            {
                var v = new Versioning(_connectionInfo, _dbConnectionFactory, _provider, _providerMetadata, _options.VersioningTableName);
                if (_customBootstrapper != null && !v.VersioningTableExists)
                {
                    ApplyCustomBootstrapping(v, assembly);
                }
                versioning = v;
            }
            return versioning;
        }

        private void ApplyCustomBootstrapping(Versioning versioning, Assembly assembly)
        {
            using (IDbConnection connection = _dbConnectionFactory.OpenConnection(_connectionInfo))
            {
                using (IDbTransaction transaction = _connectionInfo.SupportsTransactions ? connection.BeginTransaction() : null)
                {
                    _customBootstrapper.BeginBootstrapping(connection, transaction);

                    // bootstrapping is a "global" operation; therefore we need to call IsContained on *all* migrations
                    var allMigrations = CollectAllMigrationsForModule(assembly, s => true)
                        .Select(m => m.Metadata);
                    var migrationsContainedAtBootstrapping = from m in allMigrations
                                                             where _customBootstrapper.IsContained(m)
                                                             select m;
                    versioning.UpdateToInclude(migrationsContainedAtBootstrapping, connection, transaction);
                    _customBootstrapper.EndBootstrapping(connection, transaction);
                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Injects a custom version mechanism.
        /// </summary>
        public void UseCustomVersioning(IVersioning customVersioning)
        {
            if (customVersioning == null) throw new ArgumentNullException("customVersioning");
            if (_customBootstrapper != null) throw new InvalidOperationException("Either use custom versioning or custom bootstrapping.");

            _customVersioning = customVersioning;
        }

        /// <summary>
        /// Injects a custom bootstrapping mechanism.
        /// </summary>
        public void UseCustomBootstrapping(IBootstrapper customBootstrapper) // signature used in Wiki Manual
        {
            if (customBootstrapper == null) throw new ArgumentNullException("customBootstrapper");
            if (_customVersioning != null) throw new InvalidOperationException("Either use custom versioning or custom bootstrapping.");

            _customBootstrapper = customBootstrapper;
        }

        private static List<MigrationInfo> CollectAllMigrationsForModule(Assembly assembly, Predicate<string> includeModule)
        {
            Log.Info("Collecting all migrations from assembly {0}...", assembly.FullName);
            var catalog = new AssemblyCatalog(assembly);
            var container = new CompositionContainer(catalog);
            var migrationImporter = new MigrationImporter();
            container.ComposeParts(migrationImporter);
            List<MigrationInfo> result =
                new List<MigrationInfo>(
                    migrationImporter.Migrations
                        .Where(l => includeModule(l.Metadata.ModuleName))
                        .Select(l => new MigrationInfo(l.Value, new MigrationMetadata(l.Metadata.Tag, l.Metadata.ModuleName, l.Value.GetType().GetTimestamp()))));
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

        private class MigrationInfo
        {
            private readonly IMigration _migration;
            private readonly IMigrationMetadata _metadata;

            public IMigration Migration { get { return _migration; } }
            public IMigrationMetadata Metadata { get { return _metadata; } }

            public MigrationInfo(IMigration migration, IMigrationMetadata metadata)
            {
                _migration = migration;
                _metadata = metadata;
            }
        }
    }
}