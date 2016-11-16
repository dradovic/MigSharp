using System;
using System.Linq;
using MigSharp.Core;
using MigSharp.Process;

namespace MigSharp
{
    /// <summary>
    /// Exposes the API to alter database schemas without taking account of versioning.
    /// </summary>
    public class DbSchema : DbAlterer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DbSchema"/>.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dbPlatform"></param>
        /// <param name="options"></param>
        public DbSchema(string connectionString, DbPlatform dbPlatform, DbAltererOptions options)
            : base(connectionString, dbPlatform, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DbSchema"/> with default options.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dbPlatform"></param>
        public DbSchema(string connectionString, DbPlatform dbPlatform)
            : this(connectionString, dbPlatform, new DbAltererOptions())
        {
        }

        /// <summary>
        /// Alters the database schema as specified using the fluent interface on the passed <see cref="IDatabase"/>.
        /// </summary>
        public void Alter(Action<IDatabase> alterDatabase)
        {
            var migration = new AlterSchemaMigration(alterDatabase);
            Alter(migration);
        }

        /// <summary>
        /// Alters the database schema by applying the specified migration. Versioning is unaffected by this operation and any timestamp information on the <paramref name="migration"/> is disregarded.
        /// </summary>
        public void Alter(IMigration migration)
        {
            if (migration is IReversibleMigration)
            {
                Log.Info(LogCategory.General, "Migrations used to modify the database schema directly cannot be reversed.");
            }

            var migrationMetadata = new MigrationMetadata(0, "Bypass", "This migration is being executed without affecting the versioning.");
            var stepMetadata = new MigrationStepMetadata(MigrationDirection.Up, false, new[] { migrationMetadata });
            var batch = new MigrationBatch(new[]
            {
                new MigrationStep(migration, stepMetadata)
            }, Enumerable.Empty<IMigrationMetadata>(), new NoVersioning(), Configuration);
            batch.Execute();
        }

        private class AlterSchemaMigration : IMigration
        {
            private readonly Action<IDatabase> _alterDatabase;

            public AlterSchemaMigration(Action<IDatabase> alterDatabase)
            {
                _alterDatabase = alterDatabase;
            }

            public void Up(IDatabase db)
            {
                _alterDatabase(db);
            }
        }
    }
}