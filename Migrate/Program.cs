using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using MigSharp.Migrate.Util;

namespace MigSharp.Migrate
{
    internal class Program
    {
        private const int SuccessExitCode = 0x0;
        public const int InvalidArgumentsExitCode = 0x1;
        public const int InvalidTargetExitCode = 0x2;
        public const int FailedMigrationExitCode = 0x10;

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
            DbPlatform dbPlatform;
            string assemblyPath;
            string[] additionalAssemblyPaths;
            long timestamp;
            SourceLevels traceLevels;
            MigrationOptions options;
            try
            {
                options = ParseCommandLineArguments(commandLineOptions, parser, ConfigurationManager.ConnectionStrings, out connectionString, out dbPlatform, out assemblyPath, out additionalAssemblyPaths, out timestamp, out traceLevels);
            }
            catch (InvalidCommandLineArgumentException x)
            {
                Console.Error.WriteLine(x.Message);
                Console.Error.WriteLine("The provided command line was: {0}", Environment.CommandLine);
                Environment.Exit(x.ExitCode);
                throw; // will not be executed; just to satisfy R#
            }

            Trace.Listeners.Add(new ConsoleTraceListener()); // IMPORTANT: do this before setting the trace levels
            Options.SetGeneralTraceLevel(traceLevels);
            Options.SetSqlTraceLevel(traceLevels);
            Options.SetPerformanceTraceLevel(traceLevels);

            try
            {
                ExecuteMigration(connectionString, dbPlatform, options, assemblyPath, timestamp, additionalAssemblyPaths);
            }
            catch (Exception x)
            {
                Console.Error.WriteLine("Failed to migrate: {0}", GetErrorMessage(x));
                Environment.Exit(FailedMigrationExitCode);
            }
        }

        internal static bool DisplayHelp(string commandLine, out CommandLineOptions options, out CommandLineParser parser)
        {
            options = new CommandLineOptions();
            parser = new CommandLineParser(commandLine, options);
            parser.Parse();

            return options.Help;
        }

        internal static MigrationOptions ParseCommandLineArguments(CommandLineOptions options, CommandLineParser parser, ConnectionStringSettingsCollection connectionStrings,
            out string connectionString, out DbPlatform dbPlatform, out string assemblyPath, out string[] additionalAssemblyPaths, out long timestamp, out SourceLevels traceLevels)
        {
            if (parser.Parameters.Length < 2 || // expect at least the target and one assembly
                parser.UnhandledSwitches.Length > 0)
            {
                throw new InvalidCommandLineArgumentException("Invalid command line arguments. Specify at least the target and one assembly." + Environment.NewLine + Environment.NewLine + GetUsageMessage(parser),
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

            // provider name
            dbPlatform = new DbPlatform(options.Platform, options.MajorVersion, options.Driver);

            // assembly paths
            assemblyPath = parser.Parameters[1];
            additionalAssemblyPaths = parser.Parameters.Skip(2).ToArray();

            // timestamp
            timestamp = long.MaxValue;
            if (options.To != null)
            {
                try
                {
                    timestamp = long.Parse(options.To, CultureInfo.CurrentCulture);
                }
                catch (FormatException x)
                {
                    throw new InvalidCommandLineArgumentException(string.Format(CultureInfo.CurrentCulture,
                        "Could not parse timestamp: '{0}': {1}", options.To, x.Message),
                        InvalidArgumentsExitCode);
                }
            }

            // trace level
            traceLevels = SourceLevels.Warning;
            if (!string.IsNullOrEmpty(options.TraceLevel))
            {
                try
                {
                    traceLevels = (SourceLevels)Enum.Parse(typeof(SourceLevels), options.TraceLevel, true);
                }
                catch (ArgumentException x)
                {
                    throw new InvalidCommandLineArgumentException(string.Format(CultureInfo.CurrentCulture,
                        "Could not parse traceLevel: '{0}': {1}", options.TraceLevel, x.Message),
                        InvalidArgumentsExitCode);
                }
            }

            //
            // other migration options
            //
            var migrationOptions = !string.IsNullOrEmpty(options.Module) ? new MigrationOptions(options.Module) : new MigrationOptions();

            // scripting
            if (!string.IsNullOrEmpty(options.ScriptTo))
            {
                if (options.ScriptOnly)
                {
                    migrationOptions.OnlyScriptSqlTo(options.ScriptTo);
                }
                else
                {
                    migrationOptions.ExecuteAndScriptSqlTo(options.ScriptTo);
                }
            }
            else
            {
                if (options.ScriptOnly)
                {
                    throw new InvalidCommandLineArgumentException("The -scriptOnly switch requires a -scriptTo argument.",
                        InvalidArgumentsExitCode);
                }
            }

            // versioning table
            if (!string.IsNullOrEmpty(options.VersioningTable))
            {
                migrationOptions.VersioningTableName = options.VersioningTable;
            }

            return migrationOptions;
        }

        private static void ExecuteMigration(string connectionString, DbPlatform dbPlatform, MigrationOptions options, string assemblyPath, long timestamp, string[] additionalAssemblyPaths)
        {
            var migrator = new Migrator(connectionString, dbPlatform, options);
            IMigrationBatch batch = migrator.FetchMigrationsTo(assemblyPath, timestamp, additionalAssemblyPaths);
            batch.Execute();
        }

        private static string GetUsageMessage(CommandLineParser parser)
        {
            string usage = "Migrate.exe <target> <assembly> [..<assembly>] [<Arguments>]" + Environment.NewLine + Environment.NewLine +
                           "target:     name of the connectionString as specified in Migrate.exe.config" + Environment.NewLine +
                           "assembly:   space separated path(s) to the assembly dll(s) containing the migrations to execute" + Environment.NewLine +
                           Environment.NewLine + parser.GetUsage();
            return string.Format(CultureInfo.CurrentCulture, "Usage:{0}{1}", Environment.NewLine, usage);
        }

        private static string GetErrorMessage(Exception exception)
        {
            string message = Environment.NewLine;
            Exception x = exception;
            while (x != null)
            {
                message += string.Format(CultureInfo.CurrentCulture, "{0}: {1}{2}{3}{2}{2}", x.GetType(), x.Message, Environment.NewLine, x.StackTrace);
                x = x.InnerException;
            }
            return message;
        }
    }
}