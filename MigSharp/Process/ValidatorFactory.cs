using System.Collections.Generic;
using System.Linq;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class ValidatorFactory
    {
        private readonly ProviderInfo _providerInfo;
        private readonly DbAltererOptions _options;
        private readonly ProviderLocator _providerLocator;

        public ValidatorFactory(ProviderInfo providerInfo, DbAltererOptions options, ProviderLocator providerLocator)
        {
            _providerInfo = providerInfo;
            _options = options;
            _providerLocator = providerLocator;
        }

        public Validator Create()
        {
            List<ProviderInfo> providerInfos = new List<ProviderInfo>();
            if (_options.Validate)
            {
                foreach (DbPlatform name in _options.SupportedPlatforms)
                {
                    providerInfos.AddRange(_providerLocator.GetAllForMinimumRequirement(name));
                }
            }
            if (!providerInfos.Any(i => i.Metadata.Platform == _providerInfo.Metadata.Platform &&
                                        i.Metadata.MajorVersion == _providerInfo.Metadata.MajorVersion &&
                                        i.Metadata.Driver == _providerInfo.Metadata.Driver))
            {
                providerInfos.Add(_providerInfo);
            }
            return new Validator(providerInfos, _options);
        }
    }
}