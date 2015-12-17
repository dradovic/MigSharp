using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using MigSharp.Core;

namespace MigSharp.Process
{
    internal class MigrationSelector
    {
        private readonly IEnumerable<ImportedMigration> _importedMigrations;
        private readonly IEnumerable<IMigrationMetadata> _executedMigrations;

        public MigrationSelector(IEnumerable<ImportedMigration> importedMigrations, IEnumerable<IMigrationMetadata> executedMigrations)
        {
            var duplicateMigration = importedMigrations
                .GroupBy(m => m.Metadata, (metadata, enumerable) => new { metadata.Timestamp, metadata.ModuleName, Count = enumerable.Count() }, new MigrationMetadataComparer())
                .FirstOrDefault(m => m.Count > 1);
            if (duplicateMigration != null)
            {
                throw new InvalidMigrationExportException(string.Format(CultureInfo.CurrentCulture,
                    "The migration with timestamp {0} and module name '{1}' is defined more than once.",
                    duplicateMigration.Timestamp,
                    duplicateMigration.ModuleName));
            }

            _importedMigrations = importedMigrations;
            _executedMigrations = executedMigrations;
        }

        public void GetMigrationsTo(long timestamp, Predicate<string> moduleSelector,
            out IEnumerable<ApplicableMigration> applicableMigrations, out IEnumerable<IMigrationMetadata> unidentifiedMigrations)
        {
            var moduleMigrations = from m in _importedMigrations where moduleSelector(m.Metadata.ModuleName) select m;

            var comparer = new MigrationMetadataComparer();
            var applicableUpMigrations = new List<ApplicableMigration>(
                from m in moduleMigrations
                where m.Metadata.Timestamp <= timestamp &&
                      !_executedMigrations.Any(x => comparer.Equals(x, m.Metadata))
                orderby m.Metadata.Timestamp ascending
                select new ApplicableMigration(m.Implementation, new ScheduledMigrationMetadata(m.Metadata.Timestamp, m.Metadata.ModuleName, m.Metadata.Tag, MigrationDirection.Up, m.UseModuleNameAsDefaultSchema)));

            var applicableDownMigrations = new List<ApplicableMigration>(
                from m in moduleMigrations
                where m.Metadata.Timestamp > timestamp &&
                      _executedMigrations.Any(x => comparer.Equals(x, m.Metadata))
                orderby m.Metadata.Timestamp descending
                select new ApplicableMigration(m.Implementation, new ScheduledMigrationMetadata(m.Metadata.Timestamp, m.Metadata.ModuleName, m.Metadata.Tag, MigrationDirection.Down, m.UseModuleNameAsDefaultSchema)));

            if (applicableDownMigrations.Any(m => !(m.Implementation is IReversibleMigration)))
            {
                throw new IrreversibleMigrationException();
            }
            int countUp = applicableUpMigrations.Count();
            int countDown = applicableDownMigrations.Count();
            Log.Info("Found {0} (up: {1}, down: {2}) applicable migration(s)", countUp + countDown, countUp, countDown);
            applicableMigrations = applicableDownMigrations.Concat(applicableUpMigrations); // order matters!

            unidentifiedMigrations = new List<IMigrationMetadata>(
                from m in _executedMigrations
                where !_importedMigrations.Any(a => a.Metadata.ModuleName == m.ModuleName &&
                                                    a.Metadata.Timestamp == m.Timestamp)
                orderby m.Timestamp
                select m);
            if (unidentifiedMigrations.Any())
            {
                Log.Warning("Found {0} migration(s) that were executed in the database but are not contained in the application.", unidentifiedMigrations.Count());
            }
        }

        private class MigrationMetadataComparer : IEqualityComparer<IMigrationMetadata>
        {
            public bool Equals(IMigrationMetadata x, IMigrationMetadata y)
            {
                return x.Timestamp == y.Timestamp && x.ModuleName == y.ModuleName;
            }

            public int GetHashCode(IMigrationMetadata obj)
            {
                return obj.Timestamp.GetHashCode() ^ obj.ModuleName.GetHashCode();
            }
        }
    }
}