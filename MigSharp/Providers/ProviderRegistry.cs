using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MigSharp.Providers
{
    internal class ProviderRegistry : IProviderRegistry
    {
        private static readonly Lazy<IEnumerable<Lazy<IProvider, IProviderMetadata>>> Providers = new Lazy<IEnumerable<Lazy<IProvider, IProviderMetadata>>>(
            () =>
            {
                var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                var container = new CompositionContainer(catalog);
                return container.GetExports<IProvider, IProviderMetadata>();
            },
            LazyThreadSafetyMode.PublicationOnly
        );

        public IProvider GetProvider(IProviderMetadata metadata)
        {
            return Providers.Value.Single(p => p.Metadata == metadata).Value;
        }

        public IEnumerable<IProviderMetadata> GetProviderMetadatas()
        {
            return Providers.Value.Select(p => p.Metadata);
        }
    }
}