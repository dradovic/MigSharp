using System;

namespace MigSharp.Providers
{
    internal class ProviderInfo
    {
        public IProvider Provider { get; private set; }

        public IProviderMetadata Metadata { get; private set; }

        public ProviderInfo(IProvider provider, IProviderMetadata metadata)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (metadata == null) throw new ArgumentNullException("metadata");
            
            Provider = provider;
            Metadata = metadata;
        }
    }
}