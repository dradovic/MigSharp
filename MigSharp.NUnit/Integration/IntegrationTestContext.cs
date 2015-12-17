using System.Collections.Generic;
using System.Data;
using MigSharp.Process;
using MigSharp.Providers;
using System.Linq;

namespace MigSharp.NUnit.Integration
{
    internal static class IntegrationTestContext
    {
        private static MigrationOptions _options;
        private static ProviderInfo _providerInfo;

        internal static IProviderMetadata ProviderMetadata { get { return _providerInfo.Metadata; }}

        internal static void Initialize(MigrationOptions options, ProviderInfo providerInfo)
        {
            _options = options;
            _providerInfo = providerInfo;
        }

        /// <summary>
        /// Gets or sets the supported data types of the provider of the currently executing integration test.
        /// </summary>
        public static IEnumerable<SupportsAttribute> SupportsAttributes { get { return _providerInfo.Provider.GetSupportsAttributes(); } }

        public static bool IsScripting { get { return _options.ScriptingOptions.Mode == ScriptingMode.ScriptAndExecute || _options.ScriptingOptions.Mode == ScriptingMode.ScriptOnly; } }

        public static bool ProviderSupports(DbType dbType)
        {
            return SupportsAttributes.Any(a => a.DbType == dbType);
        }
    }
}