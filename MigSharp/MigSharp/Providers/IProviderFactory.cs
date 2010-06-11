namespace MigSharp.Providers
{
    internal interface IProviderFactory
    {
        IProvider GetProvider(string providerInvariantName);
    }
}