using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace MigSharp.Providers
{
    internal class ProviderLocator
    {
        private readonly IProviderRegistry _providerRegistry;

        public ProviderLocator(IProviderRegistry providerRegistry)
        {
            _providerRegistry = providerRegistry;
        }

        [Pure, NotNull]
        public ProviderInfo GetLatest(DbPlatform dbPlatform)
        {
            IProviderMetadata providerMetadata = FindLatest(dbPlatform);
            if (providerMetadata == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Could not find a provider for '{0}'.", dbPlatform));
            }
            return ToProviderInfo(providerMetadata);
        }

        private IProviderMetadata FindLatest(DbPlatform dbPlatform)
        {
            return FindAllVersionsOf(dbPlatform)
                .LastOrDefault(p => p.MajorVersion <= dbPlatform.MajorVersion);
        }

        [Pure, NotNull]
        public IEnumerable<ProviderInfo> GetAllForMinimumRequirement(DbPlatform dbPlatform)
        {
            var allStartingWith = FindAllStartingWith(dbPlatform).ToList();
            if (allStartingWith.Count == 0)
            {
                yield return GetLatest(dbPlatform);
            }
            else
            {
                foreach (IProviderMetadata metadata in allStartingWith)
                {
                    yield return ToProviderInfo(metadata);
                }
            }
        }

        private IEnumerable<IProviderMetadata> FindAllStartingWith(DbPlatform dbPlatform)
        {
            return FindAllVersionsOf(dbPlatform)
                .Where(m => m.MajorVersion >= dbPlatform.MajorVersion);
        }

        [Pure, NotNull]
        public ProviderInfo GetExactly(DbPlatform dbPlatform)
        {
            IProviderMetadata metadata = _providerRegistry.GetProviderMetadatas().Single(m => m.Platform == dbPlatform.Platform && m.MajorVersion == dbPlatform.MajorVersion && m.Driver == dbPlatform.Driver);
            return new ProviderInfo(_providerRegistry.GetProvider(metadata), metadata);
        }

        private IEnumerable<IProviderMetadata> FindAllVersionsOf(DbPlatform dbPlatform)
        {
            return _providerRegistry.GetProviderMetadatas()
                                   .Where(m => m.Platform == dbPlatform.Platform && m.Driver == dbPlatform.Driver)
                                   .OrderBy(m => m.MajorVersion);
        } 

        private ProviderInfo ToProviderInfo(IProviderMetadata providerMetadata)
        {
            return new ProviderInfo(_providerRegistry.GetProvider(providerMetadata), providerMetadata);
        }
    }
}