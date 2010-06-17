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
        private IEnumerable<Lazy<IProvider, IProviderMetaData>> Providers { get; set; } // set by MEF
// ReSharper restore UnusedAutoPropertyAccessor.Local

        public ProviderFactory()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        public IProvider GetProvider(string providerInvariantName, out IProviderMetaData metaData)
        {
            Lazy<IProvider, IProviderMetaData> exportedProvider = Providers.Where(p => p.Metadata.InvariantName == providerInvariantName).SingleOrDefault();
            if (exportedProvider == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Provider '{0}' is not supported.", providerInvariantName));
            }
            metaData = exportedProvider.Metadata;
            return exportedProvider.Value;
        }
    }
}