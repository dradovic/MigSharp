using System;

namespace MigSharp.Providers
{
    internal interface IProviderFactory
    {
        IProvider GetProvider(string providerInvariantName, out IProviderMetaData metaData);
    }
}