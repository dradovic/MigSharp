using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;

namespace MigSharp.Providers
{
    internal class ProviderFactory : IProviderFactory
    {
        private readonly IEnumerable<Lazy<IProvider, IProviderMetadata>> _providers;

        public ProviderFactory()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            _providers = container.GetExports<IProvider, IProviderMetadata>();
        }

        public IProvider GetProvider(IProviderMetadata metadata)
        {
            return _providers.Single(p => p.Metadata == metadata).Value;
        }

        public IEnumerable<IProviderMetadata> GetProviderMetadatas()
        {
            return _providers.Select(p => p.Metadata);
        }
    }
}