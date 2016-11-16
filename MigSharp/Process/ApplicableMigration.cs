namespace MigSharp.Process
{
    internal class ApplicableMigration
    {
        private readonly ImportedMigration _migration;
        private readonly MigrationDirection _direction;

        public ImportedMigration Migration { get { return _migration; } }
        public MigrationDirection Direction { get { return _direction; } }

        public ApplicableMigration(ImportedMigration migration, MigrationDirection direction)
        {
            _migration = migration;
            _direction = direction;
        }
    }
}