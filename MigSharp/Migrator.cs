using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MigSharp.Core;
using MigSharp.Process;

namespace MigSharp
{
    /// <summary>
    /// Represents the main entry point to perform migrations.
    /// </summary>
    public class Migrator : DbAlterer
    {
        private readonly MigrationOptions _options;

        private IVersioning _customVersioning;
        private IBootstrapper _customBootstrapper;

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="dbPlatform"></param>
        /// <param name="options">Options.</param>
        public Migrator(string connectionString, DbPlatform dbPlatform, MigrationOptions options)
            : base(connectionString, dbPlatform, options)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            if (dbPlatform == null) throw new ArgumentNullException("dbPlatform");
            if (options == null) throw new ArgumentNullException("options");

            _options = options;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/> for a specific module.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="dbPlatform"></param>
        /// <param name="moduleName">The name of the module whose migrations should be executed.</param>
        public Migrator(string connectionString, DbPlatform dbPlatform, string moduleName) :
            this(connectionString, dbPlatform, new MigrationOptions(moduleName))
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Migrator"/> with default options.
        /// </summary>
        /// <param name="connectionString">Connection string to the database to be migrated.</param>
        /// <param name="dbPlatform"></param>
        public Migrator(string connectionString, DbPlatform dbPlatform) : // signature used in a Wiki example
            this(connectionString, dbPlatform, new MigrationOptions())
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
            var containerConfiguration = new ContainerConfiguration().WithAssemblies(assemblies);
            return FetchMigrationsTo(containerConfiguration, timestamp);
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
            ContainerConfiguration containerConfiguration = new ContainerConfiguration().WithAssemblies(LoadAssemblies(assemblyPaths));
            return FetchMigrationsTo(containerConfiguration, timestamp);
        }

        private IMigrationBatch FetchMigrationsTo(ContainerConfiguration containerConfiguration, long timestamp)
        {
            IVersioning versioning = InitializeVersioning(containerConfiguration);
            IDictionary<string, IMigrationTimestampProvider> timestampProviders = InitializeTimestampProviders(containerConfiguration);
            var importer = new MigrationImporter(containerConfiguration, timestampProviders);
            var batchPreparer = new MigrationBatchPreparer(importer, versioning, Configuration);
            return batchPreparer.Prepare(timestamp, _options);
        }

        private static IDictionary<string, IMigrationTimestampProvider> InitializeTimestampProviders(ContainerConfiguration containerConfiguration)
        {
            // get timestamp providers from the MEF catalog
            using (CompositionHost container = containerConfiguration.CreateContainer())
            {
                var timestampProviderFactories = container.GetExports<ExportFactory<IMigrationTimestampProvider, MigrationTimestampProviderMetadata>>();
                var result = new Dictionary<string, IMigrationTimestampProvider>();
                foreach (var factory in timestampProviderFactories)
                {
                    if (result.ContainsKey(factory.Metadata.ModuleName))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot have more than one timestamp provider responsible for module: '{0}'.", factory.Metadata.ModuleName));
                    }
                    else
                    {
                        result.Add(factory.Metadata.ModuleName, factory.CreateExport().Value);
                    }
                }
                // add default timestamp provider if needed
                if (!result.ContainsKey(MigrationExportAttribute.DefaultModuleName))
                {
                    result.Add(MigrationExportAttribute.DefaultModuleName, new DefaultMigrationTimestampProvider());
                }
                return result;
            }
        }

        private IVersioning InitializeVersioning(ContainerConfiguration containerConfiguration)
        {
            IVersioning versioning;
            if (_customVersioning != null)
            {
                versioning = _customVersioning;
            }
            else
            {
                var v = new Versioning(Configuration, _options.VersioningTable);
                if (_customBootstrapper != null && !v.VersioningTableExists)
                {
                    ApplyCustomBootstrapping(v, containerConfiguration);
                }
                versioning = v;
            }
            return versioning;
        }

        private void ApplyCustomBootstrapping(Versioning versioning, ContainerConfiguration containerConfiguration)
        {
            var timestampProviders = InitializeTimestampProviders(containerConfiguration);
            using (IDbConnection connection = Configuration.OpenConnection())
            {
                using (IDbTransaction transaction = Configuration.ConnectionInfo.SupportsTransactions ? connection.BeginTransaction() : null)
                {
                    _customBootstrapper.BeginBootstrapping(connection, transaction);

                    // bootstrapping is a "global" operation; therefore we need to call IsContained on *all* migrations
                    var importer = new MigrationImporter(containerConfiguration, timestampProviders);
                    var allMigrations = importer.ImportMigrations()
                        .Select(m => m.Metadata);
                    var migrationsContainedAtBootstrapping = allMigrations.Where(m => _customBootstrapper.IsContained(m));
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

        private static IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> assemblyPaths)
        {
            foreach (string assemblyPath in assemblyPaths)
            {
                Log.Info("Including migrations from assembly '{0}'", assemblyPath);
                yield return LoadAssembly(assemblyPath);
            }
        }

        private static Assembly LoadAssembly(string assemblyPath)
        {
            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
            return Assembly.Load(assemblyName);
        }
    }
}