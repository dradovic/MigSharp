using System;

namespace MigSharp.Providers
{
    internal interface IProviderFactory
    {
        IProvider GetProvider(string providerInvariantName, out IProviderMetadata metadata);
    }
}