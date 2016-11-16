using System.Collections.Generic;
using System.Linq;

namespace MigSharp.Process
{
    internal class MigrationBatchPreparer
    {
        private readonly IMigrationImporter _importer;
        private readonly IVersioning _versioning;
        private readonly IRuntimeConfiguration _configuration;

        public MigrationBatchPreparer(IMigrationImporter importer, IVersioning versioning, IRuntimeConfiguration configuration)
        {
            _importer = importer;
            _versioning = versioning;
            _configuration = configuration;
        }

        public IMigrationBatch Prepare(long timestamp, MigrationOptions options)
        {
            // import all migrations
            IReadOnlyCollection<ImportedMigration> availableMigrations;
            IReadOnlyCollection<ImportedAggregateMigration> availableAggregateMigrations;
            _importer.ImportAll(out availableMigrations, out availableAggregateMigrations);

            // create migration batch
            var executedMigrations = new List<IMigrationMetadata>(_versioning.ExecutedMigrations);
            var migrationSelector = new MigrationSelector(availableMigrations, executedMigrations);
            IEnumerable<ApplicableMigration> applicableMigrations;
            IEnumerable<IMigrationMetadata> unidentifiedMigrations;
            migrationSelector.GetMigrationsTo(timestamp, options.MigrationSelector, out applicableMigrations, out unidentifiedMigrations);
            var migrationAggregator = new MigrationAggregator(applicableMigrations, availableMigrations, availableAggregateMigrations);
            var migrationSteps = migrationAggregator.Aggregate();
            return new MigrationBatch(
                migrationSteps,
                unidentifiedMigrations,
                _versioning,
                _configuration);
        }
    }
}