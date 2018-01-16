using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MigSharp.Providers
{
    internal class ProviderRegistry : IProviderRegistry
    {
        private static readonly Lazy<IEnumerable<ExportFactory<IProvider, ProviderMetadata>>> Providers = new Lazy<IEnumerable<ExportFactory<IProvider, ProviderMetadata>>>(
            () =>
            {
                ContainerConfiguration containerConfiguration = new ContainerConfiguration().WithAssemblies(new[] { Assembly.GetExecutingAssembly() });
                using (CompositionHost container = containerConfiguration.CreateContainer())
                {
                    return container.GetExports<ExportFactory<IProvider, ProviderMetadata>>();
                }
            },
            LazyThreadSafetyMode.PublicationOnly
        );

        public IProvider GetProvider(IProviderMetadata metadata)
        {
            return Providers.Value.Single(p => p.Metadata == metadata).CreateExport().Value;
        }

        public IEnumerable<IProviderMetadata> GetProviderMetadatas()
        {
            return Providers.Value.Select(p => p.Metadata);
        }
    }
}