using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Data;
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
        /// <param name="additionalAssemblies">Optional assemblies that hold additional migrations.</param>
        public void MigrateAll(Assembly assembly, params Assembly[] additionalAssemblies) // signature used in a Wiki example
        {
            DateTime start = DateTime.Now;
            Log.Info("Migrating all...");

            IMigrationBatch batch = FetchMigrations(assembly, additionalAssemblies);
            batch.Execute();

            Log.Info(LogCategory.Performance, "All migration(s) took {0}s", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// Executes all migrations required to reach <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="timestamp"></param>
        /// <param name="additionalAssemblies">Optional assemblies that hold additional migrations.</param>
        public void MigrateTo(Assembly assembly, long timestamp, params Assembly[] additionalAssemblies)
        {
            DateTime start = DateTime.Now;
            Log.Info("Migrating to {0}...", timestamp);

            IMigrationBatch batch = FetchMigrationsTo(assembly, timestamp, additionalAssemblies);
            batch.Execute();

            Log.Info(LogCategory.Performance, "Migration(s) to {0} took {1}s", timestamp, (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// Retrieves all pending migrations.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        /// <param name="additionalAssemblies">Optional assemblies that hold additional migrations.</param>
        public IMigrationBatch FetchMigrations(Assembly assembly, params Assembly[] additionalAssemblies)
        {
            return FetchMigrationsTo(assembly, long.MaxValue, additionalAssemblies);
        }

        /// <summary>
        /// Retrieves all required migrations to reach <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="assembly">The assembly that contains the migrations.</param>
        /// <param name="timestamp">The timestamp to migrate to.</param>
        /// <exception cref="IrreversibleMigrationException">When the migration path would require downgrading a migration which is not reversible.</exception>
        /// <param name="additionalAssemblies">Optional assemblies that hold additional migrations.</param>
        public IMigrationBatch FetchMigrationsTo(Assembly assembly, long timestamp, params Assembly[] additionalAssemblies)
        {
            IEnumerable<Assembly> assemblies = new[] { assembly }.Concat(additionalAssemblies);
            ComposablePartCatalog catalog = CreateCatalog(assemblies, a => new AssemblyCatalog(a), a => a.FullName);
            return FetchMigrationsTo(catalog, timestamp);
        }

        /// <summary>
        /// Retrieves all required migrations to reach <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly that contains the migrations.</param>
        /// <param name="timestamp">The timestamp to migrate to.</param>
        /// <exception cref="IrreversibleMigrationException">When the migration path would require downgrading a migration which is not reversible.</exception>
        /// <param name="additionalAssemblyPaths">Paths to optional assemblies that hold additional migrations.</param>
        public IMigrationBatch FetchMigrationsTo(string assemblyPath, long timestamp, params string[] additionalAssemblyPaths)
        {
            IEnumerable<string> assemblyPaths = new[] { assemblyPath }.Concat(additionalAssemblyPaths);
            ComposablePartCatalog catalog = CreateCatalog(assemblyPaths, p => new AssemblyCatalog(p), p => p);
            return FetchMigrationsTo(catalog, timestamp);
        }

        private IMigrationBatch FetchMigrationsTo(ComposablePartCatalog catalog, long timestamp)
        {
            // collect all migrations
            DateTime start = DateTime.Now;
            IEnumerable<Migration> availableMigrations = CollectAllMigrations(catalog);
            Log.Verbose(LogCategory.Performance, "Collecting migrations took {0}s", (DateTime.Now - start).TotalSeconds);

            // initialize command execution/scripting dispatching
            ISqlDispatcher dispatcher = new SqlDispatcher(_options.ScriptingOptions, _provider, _providerMetadata);

            // initialize versioning component and get executed migrations
            IVersioning versioning = InitializeVersioning(catalog, dispatcher);
            var executedMigrations = new List<IMigrationMetadata>(versioning.ExecutedMigrations);

            // filter applicable migrations
            var applicableUpMigrations = new List<Migration>(
                from m in availableMigrations
                where _options.ModuleSelector(m.Metadata.ModuleName) &&
                      m.Metadata.Timestamp <= timestamp &&
                      !executedMigrations.Any(x => x.ModuleName == m.Metadata.ModuleName &&
                                                   x.Timestamp == m.Metadata.Timestamp)
                orderby m.Metadata.Timestamp ascending
                select m);
            var applicableDownMigrations = new List<Migration>(
                from m in availableMigrations
                where _options.ModuleSelector(m.Metadata.ModuleName) &&
                      m.Metadata.Timestamp > timestamp &&
                      executedMigrations.Any(x => x.ModuleName == m.Metadata.ModuleName &&
                                                  x.Timestamp == m.Metadata.Timestamp)
                orderby m.Metadata.Timestamp descending
                select m);
            if (applicableDownMigrations.Any(m => !(m.Implementation is IReversibleMigration)))
            {
                throw new IrreversibleMigrationException();
            }
            int countUp = applicableUpMigrations.Count;
            int countDown = applicableDownMigrations.Count;
            Log.Info("Found {0} (up: {1}, down: {2}) applicable migration(s)", countUp + countDown, countUp, countDown);

            var unidentifiedMigrations = new List<IMigrationMetadata>(
                from m in executedMigrations
                where !availableMigrations.Any(a => a.Metadata.ModuleName == m.ModuleName &&
                                                    a.Metadata.Timestamp == m.Timestamp)
                orderby m.Timestamp
                select m);
            if (unidentifiedMigrations.Count > 0)
            {
                Log.Warning("Found {0} migration(s) that were executed in the database but are not contained in the application.", unidentifiedMigrations.Count);
            }

            return new MigrationBatch(
// ReSharper disable RedundantEnumerableCastCall
                applicableUpMigrations.Select(m => new MigrationStep(m.Implementation, new ScheduledMigrationMetadata(m.Metadata.Timestamp, m.Metadata.ModuleName, m.Metadata.Tag, MigrationDirection.Up), _connectionInfo, _provider, _providerMetadata, _dbConnectionFactory, dispatcher)).Cast<IMigrationStep>(),
                applicableDownMigrations.Select(m => new MigrationStep(m.Implementation, new ScheduledMigrationMetadata(m.Metadata.Timestamp, m.Metadata.ModuleName, m.Metadata.Tag, MigrationDirection.Down), _connectionInfo, _provider, _providerMetadata, _dbConnectionFactory, dispatcher)).Cast<IMigrationStep>(),
// ReSharper restore RedundantEnumerableCastCall
                unidentifiedMigrations,
                versioning,
                _options);
        }

        private IVersioning InitializeVersioning(ComposablePartCatalog catalog, ISqlDispatcher dispatcher)
        {
            IVersioning versioning;
            if (_customVersioning != null)
            {
                versioning = _customVersioning;
            }
            else
            {
                var v = new Versioning(_connectionInfo, _dbConnectionFactory, _provider, _providerMetadata, _options.VersioningTableName, dispatcher);
                if (_customBootstrapper != null && !v.VersioningTableExists)
                {
                    ApplyCustomBootstrapping(v, catalog);
                }
                versioning = v;
            }
            return versioning;
        }

        private void ApplyCustomBootstrapping(Versioning versioning, ComposablePartCatalog catalog)
        {
            using (IDbConnection connection = _dbConnectionFactory.OpenConnection(_connectionInfo))
            {
                using (IDbTransaction transaction = _connectionInfo.SupportsTransactions ? connection.BeginTransaction() : null)
                {
                    _customBootstrapper.BeginBootstrapping(connection, transaction);

                    // bootstrapping is a "global" operation; therefore we need to call IsContained on *all* migrations
                    var allMigrations = CollectAllMigrations(catalog)
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

        private static ComposablePartCatalog CreateCatalog<T>(IEnumerable<T> assemblies, Func<T, ComposablePartCatalog> createCatalogFor, Func<T, string> getAssemblyName)
        {
            var catalog = new AggregateCatalog();
            foreach (T assembly in assemblies)
            {
                Log.Info("Including migrations from assembly '{0}'", getAssemblyName(assembly));
                catalog.Catalogs.Add(createCatalogFor(assembly));
            }
            return catalog;
        }

        private static IEnumerable<Migration> CollectAllMigrations(ComposablePartCatalog catalog)
        {
            Log.Info("Collecting migrations...");
            var container = new CompositionContainer(catalog);
            IEnumerable<Lazy<IMigration, IMigrationExportMetadata>> migrations = container.GetExports<IMigration, IMigrationExportMetadata>();
            List<Migration> result =
                new List<Migration>(migrations
                    .Select(l => new Migration(l.Value, new MigrationMetadata(l.Value.GetType().GetTimestamp(), l.Metadata.ModuleName, l.Metadata.Tag))));
            Log.Info("Found {0} migration(s)", result.Count);
            return result;
        }

        private class Migration
        {
            private readonly IMigration _implementation;
            private readonly IMigrationMetadata _metadata;

            public IMigration Implementation { get { return _implementation; } }
            public IMigrationMetadata Metadata { get { return _metadata; } }

            public Migration(IMigration implementation, IMigrationMetadata metadata)
            {
                _implementation = implementation;
                _metadata = metadata;
            }
        }
    }
}