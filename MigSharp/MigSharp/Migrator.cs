using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

using MigSharp.Core;
using MigSharp.Versioning;

namespace MigSharp
{
    public class Migrator
    {
        private readonly string _connectionString;

        public Migrator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void UpgradeAll(Assembly assembly)
        {
            Log.Info("Upgrading all...");
            List<Lazy<IMigration, IMigrationMetaData>> migrations = Collect(assembly);
            if (migrations.Count > 0)
            {
                var process = new MigrationProcess(_connectionString, migrations);
                var dbVersion = DbVersion.Create(_connectionString);
                process.Do(dbVersion);
            }
        }

        private static List<Lazy<IMigration, IMigrationMetaData>> Collect(Assembly assembly)
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
            [ImportMany]
// ReSharper disable UnusedAutoPropertyAccessor.Local
            public IEnumerable<Lazy<IMigration, IMigrationMetaData>> Migrations { get; set; } // set by MEF
// ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}