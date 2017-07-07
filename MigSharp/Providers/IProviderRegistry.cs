using System.Collections.Generic;

namespace MigSharp.Providers
{
    internal interface IProviderRegistry
    {
        IProvider GetProvider(IProviderMetadata metadata);
        IEnumerable<IProviderMetadata> GetProviderMetadatas();
    }
}