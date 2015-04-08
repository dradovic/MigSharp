using System;
using System.Configuration;
using System.Globalization;
using MigSharp.Generate.Util;

namespace MigSharp.Generate
{
    internal class Program
    {
        private const int SuccessExitCode = 0x0;
        public const int InvalidArgumentsExitCode = 0x1;
        public const int InvalidTargetExitCode = 0x2;

        private static void Main()
        {
            CommandLineOptions commandLineOptions;
            CommandLineParser parser;
            if (DisplayHelp(Environment.CommandLine, out commandLineOptions, out parser))
            {
                Console.WriteLine(GetUsageMessage(parser));
                Environment.Exit(SuccessExitCode);
            }

            string connectionString;
            try
            {
                ParseCommandLineArguments(commandLineOptions, parser, ConfigurationManager.ConnectionStrings, out connectionString);
            }
            catch (InvalidCommandLineArgumentException x)
            {
                Console.Error.WriteLine(x.Message);
                Environment.Exit(x.ExitCode);
                throw; // will not be executed; just to satisfy R#
            }

        }

        internal static void ParseCommandLineArguments(CommandLineOptions options, CommandLineParser parser, ConnectionStringSettingsCollection connectionStrings, out string connectionString)
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
            string usage = "Migrate.exe <target> <assembly> [..<assembly>] [<Arguments>]" + Environment.NewLine + Environment.NewLine +
                           "target:     name of the connectionString as specified in Migrate.exe.config" + Environment.NewLine +
                           "assembly:   space separated path(s) to the assembly dll(s) containing the migrations to execute" + Environment.NewLine +
                           Environment.NewLine + parser.GetUsage();
            return string.Format(CultureInfo.CurrentCulture, "Usage:{0}{1}", Environment.NewLine, usage);
        }


    }
}