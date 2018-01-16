using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using MigSharp.Core;

namespace MigSharp.Process
{
    internal class MigrationImporter : IMigrationImporter
    {
        private readonly ContainerConfiguration _containerConfiguration;
        private readonly IDictionary<string, IMigrationTimestampProvider> _timestampProviders;

        public MigrationImporter(ContainerConfiguration containerConfiguration, IDictionary<string, IMigrationTimestampProvider> timestampProviders)
        {
            _containerConfiguration = containerConfiguration;
            _timestampProviders = timestampProviders;
        }

        public void ImportAll(out IReadOnlyCollection<ImportedMigration> migrations, out IReadOnlyCollection<ImportedAggregateMigration> aggregateMigrations)
        {
            Log.Info("Importing migrations...");
            DateTime start = DateTime.Now;
            using (CompositionHost container = _containerConfiguration.CreateContainer())
            {
                var migrationExportFactories = container.GetExports<ExportFactory<IMigration, MigrationExportMetadata>>();
                var aggregateMigrationExportFactories = container.GetExports<ExportFactory<IMigration, AggregateMigrationExportMetadata>>(AggregateMigrationExportAttribute.ContractName);
                migrations = migrationExportFactories
                    .Select(f => new { f.Metadata, f.CreateExport().Value })
                    .Select(l =>
                    {
                        var timestamp = ExtractTimestamp(l.Metadata.ModuleName, l.Value);
                        var migrationMetadata = new MigrationMetadata(timestamp, l.Metadata.ModuleName, l.Metadata.Tag);
                        return new ImportedMigration(l.Value, migrationMetadata, l.Metadata.UseModuleNameAsDefaultSchema);
                    }).ToList();
                aggregateMigrations = aggregateMigrationExportFactories
                    .Select(f => new { f.Metadata, f.CreateExport().Value })
                    .Select(l =>
                    {
                        var timestamp = ExtractTimestamp(l.Metadata.ModuleName, l.Value);
                        var aggregateMigrationMetadata = new AggregateMigrationMetadata(timestamp, l.Metadata.ModuleName);
                        return new ImportedAggregateMigration(l.Value, aggregateMigrationMetadata);
                    }).ToList();
            }
            Log.Verbose(LogCategory.Performance, "Importing migrations took {0}s", (DateTime.Now - start).TotalSeconds);
            Log.Info("Found {0} migration(s) and {1} aggregate migration(s)", migrations.Count, aggregateMigrations.Count);
        }

        public IReadOnlyCollection<ImportedMigration> ImportMigrations()
        {
            IReadOnlyCollection<ImportedMigration> migrations;
            IReadOnlyCollection<ImportedAggregateMigration> aggregateMigrations;
            ImportAll(out migrations, out aggregateMigrations);
            return migrations;
        }

        private long ExtractTimestamp(string moduleName, IMigration migration)
        {
            var timestampProvider = _timestampProviders.ContainsKey(moduleName)
                ? _timestampProviders[moduleName]
                : _timestampProviders[MigrationExportAttribute.DefaultModuleName];
            long timestamp = timestampProvider.GetTimestamp(migration.GetType());
            return timestamp;
        }
    }
}