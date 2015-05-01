using System;
using System.Configuration;
using System.Globalization;
using System.Windows.Forms;
using MigSharp.Generate.Util;
using System.Linq;

namespace MigSharp.Generate
{
    internal class Program
    {
        private const int SuccessExitCode = 0x0;
        public const int InvalidArgumentsExitCode = 0x1;
        public const int InvalidTargetExitCode = 0x2;
        public const int UnsuccessfulExitCode = 0x3;

        [STAThreadAttribute] // for setting the clipboard
        private static void Main()
        {
            CommandLineOptions options;
            CommandLineParser parser;
            if (DisplayHelp(Environment.CommandLine, out options, out parser))
            {
                Console.WriteLine(GetUsageMessage(parser));
                Environment.Exit(SuccessExitCode);
            }

            string connectionString;
            string[] excludedTables;
            try
            {
                ParseCommandLineArguments(options, parser, ConfigurationManager.ConnectionStrings, out connectionString, out excludedTables);
            }
            catch (InvalidCommandLineArgumentException x)
            {
                Console.Error.WriteLine(x.Message);
                Environment.Exit(x.ExitCode);
                throw; // will not be executed; just to satisfy R#
            }
            var generator = new SqlMigrationGenerator(connectionString, excludedTables);
            string migration = generator.Generate();
            if (generator.Errors.Any())
            {
                Console.Error.WriteLine("Following errors have occured:");
                Console.Error.WriteLine(string.Join(Environment.NewLine, generator.Errors));
                Environment.Exit(UnsuccessfulExitCode);
            }
            
            Clipboard.SetText(migration, TextDataFormat.UnicodeText);
            Console.WriteLine("The generation of the migration was successful and is now available in your clipboard.");
        }

        internal static void ParseCommandLineArguments(CommandLineOptions options, CommandLineParser parser, ConnectionStringSettingsCollection connectionStrings, out string connectionString, out string[] excludedTables)
        {
            if (parser.Parameters.Length < 1 || // expect at least the target
                parser.UnhandledSwitches.Length > 0)
            {
                throw new InvalidCommandLineArgumentException("Invalid command line arguments. Specify the target." + Environment.NewLine + Environment.NewLine + GetUsageMessage(parser),
                    InvalidArgumentsExitCode);
            }

            // connection string
            string target = parser.Parameters[0];
            ConnectionStringSettings settings = connectionStrings[target];
            if (settings == null)
            {
                throw new InvalidCommandLineArgumentException(string.Format(CultureInfo.CurrentCulture,
                    "Missing target: '{0}'. Could not find entry in the configuration file.", target),
                    InvalidTargetExitCode);
            }
            connectionString = settings.ConnectionString;
            if (connectionString == null)
            {
                throw new InvalidCommandLineArgumentException(string.Format(CultureInfo.CurrentCulture,
                    "Empty target: '{0}'. The entry in the configuration file is empty.", target),
                    InvalidTargetExitCode);
            }

            // additional parameters
            excludedTables = (options.Exclude ?? string.Empty).Split(',');
        }

        private static bool DisplayHelp(string commandLine, out CommandLineOptions options, out CommandLineParser parser)
        {
            options = new CommandLineOptions();
            parser = new CommandLineParser(commandLine, options);
            parser.Parse();

            return options.Help;
        }

        private static string GetUsageMessage(CommandLineParser parser)
        {
            string usage = "Migrate.exe <target> [<Arguments>]" + Environment.NewLine + Environment.NewLine +
                           "target:     name of the connectionString as specified in Generate.exe.config" + Environment.NewLine +
                           Environment.NewLine + parser.GetUsage();
            return string.Format(CultureInfo.CurrentCulture, "Usage:{0}{1}", Environment.NewLine, usage);
        }
    }
}