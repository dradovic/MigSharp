namespace MigSharp.Core
{
    internal class ScheduledMigrationMetadata : MigrationMetadata, IScheduledMigrationMetadata
    {
        private readonly MigrationDirection _direction;

        public MigrationDirection Direction { get { return _direction; } }

        public ScheduledMigrationMetadata(long timestamp, string moduleName, string tag, MigrationDirection direction) : base(timestamp, moduleName, tag)
        {
            _direction = direction;
        }
    }
}