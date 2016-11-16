namespace MigSharp.Process
{
    internal class MigrationContext : IMigrationContext
    {
        private readonly IProviderMetadata _providerMetadata;
        private readonly IMigrationStepMetadata _stepMetadata;

        public IProviderMetadata ProviderMetadata { get { return _providerMetadata; } }
        public IMigrationStepMetadata StepMetadata { get { return _stepMetadata; } }

        public MigrationContext(IProviderMetadata providerMetadata, IMigrationStepMetadata stepMetadata)
        {
            _providerMetadata = providerMetadata;
            _stepMetadata = stepMetadata;
        }
    }
}