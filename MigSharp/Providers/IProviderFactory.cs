using System.Collections.Generic;

namespace MigSharp.Providers
{
    internal interface IProviderFactory
    {
        IProvider GetProvider(IProviderMetadata metadata);
        IEnumerable<IProviderMetadata> GetProviderMetadatas();
    }
}