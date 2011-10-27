namespace MigSharp.Process
{
    internal class ApplicableMigration
    {
        private readonly IMigration _implementation;
        private readonly IScheduledMigrationMetadata _metadata;

        public IMigration Implementation { get { return _implementation; } }
        public IScheduledMigrationMetadata Metadata { get { return _metadata; } }

        public ApplicableMigration(IMigration implementation, IScheduledMigrationMetadata metadata)
        {
            _implementation = implementation;
            _metadata = metadata;
        }
    }
}