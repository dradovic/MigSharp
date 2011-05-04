namespace MigSharp.Providers
{
    internal interface IProviderFactory
    {
        IProvider GetProvider(string providerName, out IProviderMetadata metadata);
    }
}