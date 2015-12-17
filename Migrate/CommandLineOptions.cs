using System.Diagnostics.CodeAnalysis;

using MigSharp.Migrate.Util;

namespace MigSharp.Migrate
{
    internal class CommandLineOptions
    {
        internal const Platform DefaultPlatform = Platform.SqlServer;

        private Platform _platform = DefaultPlatform;
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("platform", false, "platform name (default: SqlServer)")]
        public Platform Platform { get { return _platform; } set { _platform = value; } }

        private int _majorVersion = DbPlatform.MaximumMajorVersion; // 1000 is a major version that is not superseeded by any provider and therefore by default the latest will be taken
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("version", false, "major version of the platform (default: take latest)")]
        public int MajorVersion { get { return _majorVersion; } set { _majorVersion = value; } }

        private Driver _driver = Driver.AdoNet;
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("driver", false, "driver (default: AdoNet)")]
        public Driver Driver { get { return _driver; } set { _driver = value; } }

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