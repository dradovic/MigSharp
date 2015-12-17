namespace MigSharp.Process
{
    internal class MigrationContext : IMigrationContext
    {
        private readonly IProviderMetadata _providerMetadata;
        private readonly IScheduledMigrationMetadata _migrationMetadata;

        public IProviderMetadata ProviderMetadata { get { return _providerMetadata; } }
        public IScheduledMigrationMetadata MigrationMetadata { get { return _migrationMetadata; } }

        public MigrationContext(IProviderMetadata providerMetadata, IScheduledMigrationMetadata migrationMetadata)
        {
            _providerMetadata = providerMetadata;
            _migrationMetadata = migrationMetadata;
        }
    }
}