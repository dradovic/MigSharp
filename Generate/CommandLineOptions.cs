using System.Diagnostics.CodeAnalysis;
using MigSharp.Generate.Util;

namespace MigSharp.Generate
{
    internal class CommandLineOptions
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("help", false, "echo this usage description")]
        public bool Help { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Setter used by CommandLineParser.")]
        [CommandLineSwitch("exclude", false, "comma-separated list of table names to be excluded")]
        public string Exclude { get; set; }
    }
}