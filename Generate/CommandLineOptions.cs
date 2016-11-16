using System.Diagnostics.CodeAnalysis;
using MigSharp.Generate.Util;

namespace MigSharp.Generate
{
    internal class CommandLineOptions
    {
        public const string DefaultVersioningTableSchema = "dbo";

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("help", false, "echo this usage description")]
        public bool Help { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("namespace", true, "the namespace of the generated migration")]
        public string Namespace { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("module", false, "the module name of the generated migration")]
        public string ModuleName { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("versioningTable", false, "the table name of the versioning table (default: '" + MigrationOptions.DefaultVersioningTableName + "')")]
        public string VersioningTableName { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("versioningSchema", false, "the schema name of the versioning table (default: '" + DefaultVersioningTableSchema + "')")]
        public string VersioningTableSchema { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("schemas", false, "comma-separated list of schema names to be included (default: include all schemas)")]
        public string Schemas { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("exclude", false, "comma-separated list of table names to be excluded (default: include all tables)")]
        public string Exclude { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("data", false, "when set, data is scripted as INSERT statements (default: false)")]
        public bool IncludeData { get; set; }

        public CommandLineOptions()
        {
            ModuleName = MigrationExportAttribute.DefaultModuleName;
            VersioningTableName = MigrationOptions.DefaultVersioningTableName;
            VersioningTableSchema = DefaultVersioningTableSchema;
        }
    }
}