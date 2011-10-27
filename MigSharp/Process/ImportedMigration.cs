namespace MigSharp.Process
{
    internal class ImportedMigration
    {
        private readonly IMigration _implementation;
        private readonly IMigrationMetadata _metadata;

        public IMigration Implementation { get { return _implementation; } }
        public IMigrationMetadata Metadata { get { return _metadata; } }

        public ImportedMigration(IMigration implementation, IMigrationMetadata metadata)
        {
            _implementation = implementation;
            _metadata = metadata;
        }
    }
}