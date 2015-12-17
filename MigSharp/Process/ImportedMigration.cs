namespace MigSharp.Process
{
    internal class ImportedMigration
    {
        private readonly IMigration _implementation;
        private readonly IMigrationMetadata _metadata;
        private readonly bool _useModuleNameAsDefaultSchema;

        public IMigration Implementation { get { return _implementation; } }
        public IMigrationMetadata Metadata { get { return _metadata; } }
        public bool UseModuleNameAsDefaultSchema { get { return _useModuleNameAsDefaultSchema; } }

        public ImportedMigration(IMigration implementation, IMigrationMetadata metadata, bool useModuleNameAsDefaultSchema)
        {
            _implementation = implementation;
            _metadata = metadata;
            _useModuleNameAsDefaultSchema = useModuleNameAsDefaultSchema;
        }
    }
}