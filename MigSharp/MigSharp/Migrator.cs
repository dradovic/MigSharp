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

        public void ExecuteAll(Assembly assembly)
        {
            Log.Info("Migrating all...");
            List<IMigration> migrations = Collect(assembly);
            if (migrations.Count > 0)
            {
                var process = new MigrationProcess(_connectionString, migrations);
                var dbVersion = DbVersion.Create(_connectionString);
                process.Do(dbVersion);
            }
        }

        private static List<IMigration> Collect(Assembly assembly)
        {
            Log.Info("Collecting all migrations...");
            var catalog = new AssemblyCatalog(assembly);
            var container = new CompositionContainer(catalog);
            var migrationImporter = new MigrationImporter();
            container.ComposeParts(migrationImporter);
            var result = new List<IMigration>(migrationImporter.Migrations);
            Log.Info("Found {0} migration(s)", result.Count);
            return result;
        }

        private class MigrationImporter
        {
            [ImportMany]
// ReSharper disable UnusedAutoPropertyAccessor.Local
            public IEnumerable<IMigration> Migrations { get; set; } // set by MEF
// ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}