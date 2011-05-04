using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MigSharp.Providers
{
    internal class ProviderFactory : IProviderFactory
    {
// ReSharper disable UnusedAutoPropertyAccessor.Local
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ImportMany]
        private IEnumerable<Lazy<IProvider, IProviderMetadata>> Providers { get; set; } // set by MEF
// ReSharper restore UnusedAutoPropertyAccessor.Local

        public ProviderFactory()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
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
            Lazy<IProvider, IProviderMetadata> exportedProvider = Providers.Where(p => p.Metadata.Name == providerName).SingleOrDefault();
            if (exportedProvider == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Provider '{0}' is not supported.", providerName));
            }
            return exportedProvider;
        }
    }
}