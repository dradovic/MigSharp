using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
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

        public IProvider GetProvider(string providerName, out IProviderMetadata metadata)
        {
            Lazy<IProvider, IProviderMetadata> exportedProvider = GetExportedProvider(providerName);
            metadata = exportedProvider.Metadata;
            return exportedProvider.Value;
        }

        public IProviderMetadata GetProviderMetadata(string providerName)
        {
            return GetExportedProvider(providerName).Metadata;
        }

        private Lazy<IProvider, IProviderMetadata> GetExportedProvider(string providerName)
        {
            Lazy<IProvider, IProviderMetadata> exportedProvider = _providers.Where(p => p.Metadata.Name == providerName).SingleOrDefault();
            if (exportedProvider == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Provider '{0}' is not supported.", providerName));
            }
            return exportedProvider;
        }
    }
}