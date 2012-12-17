using System;
using System.Collections.Generic;
using System.Globalization;

using MigSharp.Providers;

using System.Linq;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of providers that should be supported for all migrations. Validation of migrations is performed
    /// against providers contained within this list.
    /// </summary>
    public class SupportedProviders
    {
        private static readonly string[] DefaultProviderNames = new[]
        {
            ProviderNames.MySqlExperimental,
            ProviderNames.Oracle,
            ProviderNames.OracleOdbc,
            ProviderNames.SQLite,
            ProviderNames.SqlServer2005,
            ProviderNames.SqlServer2005Odbc,
            ProviderNames.SqlServer2008,
            ProviderNames.SqlServer2012,
            ProviderNames.SqlServerCe35,
            ProviderNames.SqlServerCe4,
            ProviderNames.Teradata,
            ProviderNames.TeradataOdbc,
        };

        private readonly IProviderFactory _providerFactory;
        private readonly Dictionary<string, ProviderInfo> _providers = new Dictionary<string, ProviderInfo>();

        /// <summary>
        /// Gets the names of the providers.
        /// </summary>
        public IEnumerable<string> Names { get { return _providers.Keys; } }

        /// <summary>
        /// Initializes a new instance used for unit testing.
        /// </summary>
        internal SupportedProviders(IProviderFactory providerFactory)
        {
            _providerFactory = providerFactory;
        }

        /// <summary>
        /// Initializes a new instance which is ready-to-use with the default providers.
        /// </summary>
        internal SupportedProviders() : this(new ProviderFactory())
        {
            Reset();
        }

        internal IProvider GetProvider(string providerName, out IProviderMetadata metadata)
        {
            ProviderInfo info;
            if (!_providers.TryGetValue(providerName, out info))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The provider '{0}' is not enlisted in the supported providers.", providerName));
            }
            metadata = info.Metadata;
            return info.Provider;
        }

        /// <summary>
        /// Adds a provider.
        /// </summary>
        public void Add(string providerName)
        {
            if (!_providers.ContainsKey(providerName))
            {
                IProviderMetadata metadata;
                IProvider provider = _providerFactory.GetProvider(providerName, out metadata);
                _providers.Add(providerName, new ProviderInfo(provider, metadata));
            }
        }

        /// <summary>
        /// Removes a provider.
        /// </summary>
        public void Remove(string providerName)
        {
            if (_providers.ContainsKey(providerName))
            {
                _providers.Remove(providerName);
            }
        }

        /// <summary>
        /// Removes all ODBC providers.
        /// </summary>
        public void RemoveAllOdbc()
        {
            foreach (string providerName in _providers
                .Where(p => p.Value.Metadata.InvariantName == "System.Data.Odbc")
                .Select(p => p.Key)
                .ToList())
            {
                Remove(providerName);
            }
        }

        /// <summary>
        /// Sets the collection to a list of providers.
        /// </summary>
        public void Set(IEnumerable<string> providerNames)
        {
            _providers.Clear();
            foreach (string providerName in providerNames)
            {
                Add(providerName);
            }
        }

        /// <summary>
        /// Resets the collection to support all providers that are delivered with Mig#.
        /// </summary>
        public void Reset()
        {
            Set(DefaultProviderNames);
        }

        private class ProviderInfo
        {
            private readonly IProvider _provider;
            private readonly IProviderMetadata _metadata;

            public IProvider Provider { get { return _provider; } }
            public IProviderMetadata Metadata { get { return _metadata; } }

            public ProviderInfo(IProvider provider, IProviderMetadata metadata)
            {
                _provider = provider;
                _metadata = metadata;
            }
        }
    }
}