namespace MigSharp.Core
{
    internal class ScheduledMigrationMetadata : MigrationMetadata, IScheduledMigrationMetadata
    {
        private readonly MigrationDirection _direction;
        private readonly bool _useModuleNameAsDefaultSchema;

        public MigrationDirection Direction { get { return _direction; } }
        public bool UseModuleNameAsDefaultSchema { get { return _useModuleNameAsDefaultSchema; } }

        public ScheduledMigrationMetadata(long timestamp, string moduleName, string tag, MigrationDirection direction, bool useModuleNameAsDefaultSchema)
            : base(timestamp, moduleName, tag)
        {
            _direction = direction;
            _useModuleNameAsDefaultSchema = useModuleNameAsDefaultSchema;
        }
    }
}