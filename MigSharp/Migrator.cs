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
            // import all migrations
            DateTime start = DateTime.Now;
            var timestampProviders = InitializeTimestampProviders(catalog, _options.ModuleSelector);
            IEnumerable<ImportedMigration> availableMigrations = ImportAllMigrations(catalog, timestampProviders);
            Log.Verbose(LogCategory.Performance, "Importing migrations took {0}s", (DateTime.Now - start).TotalSeconds);

            // initialize command execution/scripting dispatching
            ISqlDispatcher dispatcher = new SqlDispatcher(_options.ScriptingOptions, _provider, _providerMetadata);

            // initialize versioning component and get executed migrations
            IVersioning versioning = InitializeVersioning(catalog, dispatcher);
            var executedMigrations = new List<IMigrationMetadata>(versioning.ExecutedMigrations);

            // create migration batch
            var migrationSelector = new MigrationSelector(availableMigrations, executedMigrations);
            IEnumerable<ApplicableMigration> applicableMigrations;
            IEnumerable<IMigrationMetadata> unidentifiedMigrations;
            migrationSelector.GetMigrationsTo(timestamp, _options.ModuleSelector, out applicableMigrations, out unidentifiedMigrations);
            return new MigrationBatch(
// ReSharper disable RedundantEnumerableCastCall
                applicableMigrations.Select(m => new MigrationStep(m.Implementation, m.Metadata, _connectionInfo, _provider, _providerMetadata, _dbConnectionFactory, dispatcher)).Cast<IMigrationStep>(),
// ReSharper restore RedundantEnumerableCastCall
                unidentifiedMigrations,
                versioning,
                _options);
        }

        private static IDictionary<string, IMigrationTimestampProvider> InitializeTimestampProviders(ComposablePartCatalog catalog, Predicate<string> moduleSelector)
        {
            // Get timestamp providers from the MEF catalog
            var container = new CompositionContainer(catalog);
            var providers = container.GetExports<IMigrationTimestampProvider, IMigrationTimestampProviderExportMetadata>().ToArray();
            try
            {
                var timestampProviders = providers.ToDictionary(x => x.Metadata.ModuleName, x => x.Value);
                if (!timestampProviders.ContainsKey(MigrationExportAttribute.DefaultModuleName))
                    timestampProviders.Add(MigrationExportAttribute.DefaultModuleName, new DefaultMigrationTimestampProvider());

                return timestampProviders;
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("Cannot have more than one exported timestamp provider with the same module name.");
            }
            

            /*
            Lazy<IMigrationTimestampProvider, IMigrationTimestampProviderExportMetadata> defaultProvider = null;
            Lazy<IMigrationTimestampProvider, IMigrationTimestampProviderExportMetadata> moduleProvider = null;
            foreach (var provider in timestampProviders)
            {
                if (moduleSelector(provider.Metadata.ModuleName))
                {
                    // Found a provider that is specific to the current module
                    if (moduleProvider != null)
                        throw new InvalidOperationException(
                            string.Format("There is more than one timestamp provider for the module '{0}'. Cannot have more than one timestamp provider exported for a module in an assembly.", provider.Metadata.ModuleName));

                    moduleProvider = provider;
                }
                if (MigrationExportAttribute.DefaultModuleName.Equals(provider.Metadata.ModuleName))
                {
                    // Found a default provider
                    if (defaultProvider != null)
                        throw new InvalidOperationException("Cannot have more than one default timestamp provider exported in an assembly.");

                    defaultProvider = provider;
                }
            }

            // If we have a module provider, use that
            if (moduleProvider != null)
                return moduleProvider.Value;

            // No module specific providers found, return either the default provider for the assembly or the default provider
            return defaultProvider == null
                       ? new DefaultMigrationTimestampProvider()
                       : defaultProvider.Value;
            */
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
            var timestampProviders = InitializeTimestampProviders(catalog, _options.ModuleSelector);
            using (IDbConnection connection = _dbConnectionFactory.OpenConnection(_connectionInfo))
            {
                using (IDbTransaction transaction = _connectionInfo.SupportsTransactions ? connection.BeginTransaction() : null)
                {
                    _customBootstrapper.BeginBootstrapping(connection, transaction);

                    // bootstrapping is a "global" operation; therefore we need to call IsContained on *all* migrations
                    var allMigrations = ImportAllMigrations(catalog, timestampProviders)
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

        private static IEnumerable<ImportedMigration> ImportAllMigrations(ComposablePartCatalog catalog, IDictionary<string, IMigrationTimestampProvider> timestampProviders)
        {
            Log.Info("Importing migrations...");
            var container = new CompositionContainer(catalog);
            IEnumerable<Lazy<IMigration, IMigrationExportMetadata>> migrations = container.GetExports<IMigration, IMigrationExportMetadata>();

            var result = new List<ImportedMigration>(migrations
            .Select(l =>
                        {
                            var timestampProvider = timestampProviders.ContainsKey(l.Metadata.ModuleName)
                                                        ? timestampProviders[l.Metadata.ModuleName]
                                                        : timestampProviders[MigrationExportAttribute.DefaultModuleName];

                            return new ImportedMigration(l.Value, new MigrationMetadata(timestampProvider.GetTimestamp(l.Value.GetType()), l.Metadata.ModuleName, l.Metadata.Tag));
                        }));
            Log.Info("Found {0} migration(s)", result.Count);
            return result;
            
        }
    }
}