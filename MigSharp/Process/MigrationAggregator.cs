using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MigSharp.Core;

namespace MigSharp.Process
{
    internal class MigrationAggregator
    {
        private readonly IEnumerable<ApplicableMigration> _applicableMigrations;
        private readonly IEnumerable<ImportedMigration> _availableMigrations;
        private readonly IEnumerable<ImportedAggregateMigration> _aggregateMigrations;

        public MigrationAggregator(IEnumerable<ApplicableMigration> applicableMigrations, IEnumerable<ImportedMigration> availableMigrations, IEnumerable<ImportedAggregateMigration> aggregateMigrations)
        {
            _applicableMigrations = applicableMigrations;
            _availableMigrations = availableMigrations;
            _aggregateMigrations = aggregateMigrations;
        }

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression")]
        public IEnumerable<MigrationStep> Aggregate()
        {
            Dictionary<string, Aggregation> aggregations = FindLargestApplicableAggregations();
            foreach (ApplicableMigration applicableMigration in _applicableMigrations)
            {
                MigrationStepMetadata migrationStepMetadata;
                Aggregation aggregation;
                if (aggregations.TryGetValue(applicableMigration.Migration.Metadata.ModuleName, out aggregation))
                {
                    // Note: we issue an aggregated migration step when the last migration is hit to adhere more or less 
                    // to the ordering of the migrations.
                    if (applicableMigration.Migration.Metadata.Timestamp == aggregation.LastAggregatedMigration.Timestamp)
                    {
                        // aggregate migration step
                        migrationStepMetadata = new MigrationStepMetadata(MigrationDirection.Up, false, aggregation.AggregatedMigrations);
                        Log.Info("Using {0} to aggregate {1} migration(s) from module '{2}'", aggregation.Migration.Implementation.GetName(), migrationStepMetadata.Migrations.Count(), migrationStepMetadata.ModuleName);
                        yield return new MigrationStep(aggregation.Migration.Implementation, migrationStepMetadata);
                    }
                    if (applicableMigration.Migration.Metadata.Timestamp <= aggregation.Migration.Metadata.Timestamp)
                    {
                        continue; // the migration will be included in the aggregate
                    }
                }
                // single migration step
                migrationStepMetadata = new MigrationStepMetadata(applicableMigration.Direction, applicableMigration.Migration.UseModuleNameAsDefaultSchema, new[] { MetadataFor(applicableMigration) });
                yield return new MigrationStep(applicableMigration.Migration.Implementation, migrationStepMetadata);
            }
        }

        private Dictionary<string, Aggregation> FindLargestApplicableAggregations()
        {
            var aggregations = new Dictionary<string, Aggregation>();
            foreach (string moduleName in _availableMigrations.Select(m => m.Metadata.ModuleName).Distinct())
            {
                Aggregation aggregation = FindLargestApplicableAggregation(moduleName);
                if (aggregation != null)
                {
                    aggregations.Add(moduleName, aggregation);
                }
            }
            return aggregations;
        }

        private Aggregation FindLargestApplicableAggregation(string moduleName)
        {
            foreach (ImportedAggregateMigration aggregateMigration in _aggregateMigrations
                .Where(m => m.Metadata.ModuleName == moduleName)
                .OrderByDescending(m => m.Metadata.Timestamp))
            {
                long aggregateTimestamp = aggregateMigration.Metadata.Timestamp;
                List<IMigrationMetadata> aggregatedMigrations = _availableMigrations
                    .Where(m => m.Metadata.ModuleName == moduleName && m.Metadata.Timestamp <= aggregateTimestamp)
                    .Select(m => m.Metadata)
                    .ToList();
                if (aggregatedMigrations.All(IsApplicable))
                {
                    return new Aggregation(aggregateMigration, aggregatedMigrations);
                }
            }
            return null;
        }

        private bool IsApplicable(IMigrationMetadata migration)
        {
            var comparer = new MigrationMetadataComparer();
            return _applicableMigrations.Select(m => m.Migration.Metadata).Contains(migration, comparer);
        }

        private static MigrationMetadata MetadataFor(ApplicableMigration applicableMigration)
        {
            return new MigrationMetadata(applicableMigration.Migration.Metadata.Timestamp, applicableMigration.Migration.Metadata.ModuleName, applicableMigration.Migration.Metadata.Tag);
        }

        private class Aggregation
        {
            private readonly ImportedAggregateMigration _migration;
            private readonly List<IMigrationMetadata> _aggregatedMigrations;
            private readonly IMigrationMetadata _lastAggregatedMigration;

            public ImportedAggregateMigration Migration { get { return _migration; } }
            public List<IMigrationMetadata> AggregatedMigrations { get { return _aggregatedMigrations; } }
            public IMigrationMetadata LastAggregatedMigration { get { return _lastAggregatedMigration; } }

            public Aggregation(ImportedAggregateMigration migration, List<IMigrationMetadata> aggregatedMigrations)
            {
                Debug.Assert(aggregatedMigrations.All(m => m.ModuleName == migration.Metadata.ModuleName), "All migrations within an aggregation must belong to the same module.");

                _migration = migration;
                _aggregatedMigrations = aggregatedMigrations;
                _lastAggregatedMigration = aggregatedMigrations.Last();
            }
        }
    }
}