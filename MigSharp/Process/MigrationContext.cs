namespace MigSharp.Process
{
    internal class MigrationContext : IMigrationContext
    {
        private readonly IProviderMetadata _providerMetadata;

        public IProviderMetadata ProviderMetadata { get { return _providerMetadata; } }

        public MigrationContext(IProviderMetadata providerMetadata)
        {
            _providerMetadata = providerMetadata;
        }
    }
}