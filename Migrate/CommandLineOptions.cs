using System.Diagnostics.CodeAnalysis;

using MigSharp.Migrate.Util;

namespace MigSharp.Migrate
{
    internal class CommandLineOptions
    {
        internal const string DefaultProvider = ProviderNames.SqlServer2005;

        private string _provider = DefaultProvider;
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("provider", false, "provider name (default: " + DefaultProvider + ")")]
        public string Provider { get { return _provider; } set { _provider = value; } }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("module", false, "module to migrate (default: migrate all)")]
        public string Module { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("to", false, "timestamp to migrate to (default: migrate all)")] // FEATURE: also support tags as possible -to parameters
        public string To { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("scriptTo", false, "path to directory where to output SQL scripts")]
        public string ScriptTo { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("scriptOnly", false, "when specified, migrations are only scripted and not executed (requires -scriptTo)")]
        public bool ScriptOnly { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("support", false, "a semi-colon (" + Program.SupportedProviderSeparator + ") separated list of additional providers to validate for")]
        public string Support { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("versioningTable", false, "the versioning table name (default: " + MigrationOptions.DefaultVersioningTableName + ")")]
        public string VersioningTable { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("traceLevel", false, "Error, Warning, Information, or Verbose (default: Warning)")]
        public string TraceLevel { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("help", false, "echo this usage description")]
        public bool Help { get; set; }
    }
}