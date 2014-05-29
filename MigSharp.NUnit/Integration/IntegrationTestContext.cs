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
        private static IEnumerable<SupportsAttribute> _supportsAttributes;

        internal static void Initialize(MigrationOptions options, IEnumerable<SupportsAttribute> supportsAttributes)
        {
            _options = options;
            _supportsAttributes = supportsAttributes;
        }

        /// <summary>
        /// Gets or sets the supported data types of the provider of the currently executing integration test.
        /// </summary>
        public static IEnumerable<SupportsAttribute> SupportsAttributes { get { return _supportsAttributes; } }

        public static bool IsScripting { get { return _options.ScriptingOptions.Mode == ScriptingMode.ScriptAndExecute || _options.ScriptingOptions.Mode == ScriptingMode.ScriptOnly; } }

        public static bool ProviderSupports(DbType dbType)
        {
            return SupportsAttributes.Any(a => a.DbType == dbType);
        }
    }
}