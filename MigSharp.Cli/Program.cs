using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace MigSharp.Cli
{
    public class Program
    {
        private const int SuccessExitCode = 0x0;
        public const int InvalidArgumentsExitCode = 0x1;
        public const int InvalidTargetExitCode = 0x2;
        public const int FailedMigrationExitCode = 0x10;

        public const string ConfigFilename = "MigSharp.json"; // FIXME: da, use old config file format instead of introducing a new json format

        private const string ExitCodeKeyForExceptionData = "ExitCode";

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Command("migrate", command =>
            {
                command.HelpOption("-? | -h | --help");

                var target = command.Argument("target", "name of the connectionString as specified in ???"); // FIXME: da, specified where?
                var assemblies = command.Argument("assemblies", "path(s) to the assembly dll(s) containing the migrations to execute", true);

                var platform = command.Option("--platform", "platform name (default: SqlServer)", CommandOptionType.SingleValue);
                var version = command.Option("--version", "major version of the platform (default: take latest)", CommandOptionType.SingleValue);
                var driver = command.Option("--driver", "driver (default: AdoNet)", CommandOptionType.SingleValue);
                var module = command.Option("--module", "module to migrate (default: migrate all)", CommandOptionType.SingleValue); // FIXME: da, could be multi-value?
                var to = command.Option("--to", "timestamp to migrate to (default: migrate all)", CommandOptionType.SingleValue); // FEATURE: also support tags as possible -to parameters
                var scriptTo = command.Option("--scriptTo", "path to directory where to output SQL scripts", CommandOptionType.SingleValue);
                var scriptOnly = command.Option("--scriptOnly", "when specified, migrations are only scripted and not executed (requires -scriptTo)", CommandOptionType.NoValue);
                var versioningTable = command.Option("--versioningTable", $"the versioning table name (default: {MigrationOptions.DefaultVersioningTableName})", CommandOptionType.SingleValue);
                var traceLevel = command.Option("--traceLevel", "Error, Warning, Information, or Verbose (default: Warning)", CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    // FIXME: da, fix the tracing
                    //Trace.Listeners.Add(new ConsoleTraceListener()); // IMPORTANT: do this before setting the trace levels
                    //Options.SetGeneralTraceLevel(traceLevels);
                    //Options.SetSqlTraceLevel(traceLevels);
                    //Options.SetPerformanceTraceLevel(traceLevels);

                    ExecuteMigration(GetConnectionString(command, target), GetAssemblyPaths(command, assemblies), GetDbPlatform(platform, version, driver), GetMigrationOptions(command, module, scriptTo, scriptOnly, versioningTable), GetTimestamp(command, to));
                    return SuccessExitCode;
                });
            });
            app.HelpOption("-? | -h | --help");

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException x)
            {
                Console.Error.WriteLine(x.Message);
                x.Command.ShowHelp();
                if (x.Data.Contains(ExitCodeKeyForExceptionData))
                {
                    return (int)x.Data[ExitCodeKeyForExceptionData];
                }
                return InvalidArgumentsExitCode;
            }
            catch (Exception x)
            {
                Console.Error.WriteLine("Failed to migrate: {0}", GetErrorMessage(x));
                return FailedMigrationExitCode;
            }
        }

        private static string GetConnectionString(CommandLineApplication command, CommandArgument target)
        {
            if (string.IsNullOrEmpty(target.Value))
            {
                throw new CommandParsingException(command, "The target argument is mandatory.");
            }

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile(ConfigFilename);
            var configuration = builder.Build();
            List<ConnectionStringConfiguration> connectionStringConfigurations = new List<ConnectionStringConfiguration>();
            configuration.GetSection("ConnectionStrings").Bind(connectionStringConfigurations);
            ConnectionStringConfiguration connectionStringConfiguration;
            if (!connectionStringConfigurations.ToDictionary(c => c.Name).TryGetValue(target.Value, out connectionStringConfiguration))
            {
                throw CreateParsingException(command, $"Connection string for target '{target.Value}' not found in {Path.Combine(path, ConfigFilename)}.", InvalidTargetExitCode);
            }
            return connectionStringConfiguration.ConnectionString;
        }

        private static IEnumerable<string> GetAssemblyPaths(CommandLineApplication command, CommandArgument assemblies)
        {
            if (!assemblies.Values.Any())
            {
                throw new CommandParsingException(command, "At least one assembly containing migrations must be specified.");
            }
            return assemblies.Values;
        }

        private static DbPlatform GetDbPlatform(CommandOption platform, CommandOption version, CommandOption driver)
        {
            Platform pf = platform.HasValue() ? Enum.Parse<Platform>(platform.Value()) : Platform.SqlServer;
            int majorVersion = version.HasValue() ? int.Parse(version.Value()) : DbPlatform.MaximumMajorVersion; // 1000 is a major version that is not superseeded by any provider and therefore by default the latest will be taken
            Driver d = driver.HasValue() ? Enum.Parse<Driver>(driver.Value()) : Driver.AdoNet;
            return new DbPlatform(pf, majorVersion, d);
        }

        private static MigrationOptions GetMigrationOptions(CommandLineApplication command, CommandOption module, CommandOption scriptTo, CommandOption scriptOnly, CommandOption versioningTable)
        {
            var migrationOptions = module.HasValue() ? new MigrationOptions(module.Value()) : new MigrationOptions();

            // scripting
            if (scriptTo.HasValue())
            {
                if (scriptOnly.HasValue())
                {
                    migrationOptions.OnlyScriptSqlTo(scriptTo.Value());
                }
                else
                {
                    migrationOptions.ExecuteAndScriptSqlTo(scriptTo.Value());
                }
            }
            else
            {
                if (scriptOnly.HasValue())
                {
                    throw new CommandParsingException(command, "The -scriptOnly switch requires a -scriptTo argument.");
                }
            }

            // versioning table
            if (versioningTable.HasValue())
            {
                migrationOptions.VersioningTableName = versioningTable.Value();
            }

            return migrationOptions;
        }

        private static long GetTimestamp(CommandLineApplication command, CommandOption to)
        {
            long timestamp = long.MaxValue;
            if (to.HasValue())
            {
                try
                {
                    timestamp = long.Parse(to.Value(), CultureInfo.CurrentCulture);
                }
                catch (FormatException x)
                {
                    throw new CommandParsingException(command, string.Format(CultureInfo.CurrentCulture,
                        "Could not parse timestamp: '{0}': {1}", to.Value(), x.Message));
                }
            }
            return timestamp;
        }

        private static void ExecuteMigration(string connectionString, IEnumerable<string> assemblyPaths, DbPlatform dbPlatform, MigrationOptions options, long timestamp)
        {
            var migrator = new Migrator(connectionString, dbPlatform, options);
            IMigrationBatch batch = migrator.FetchMigrationsTo(assemblyPaths.First(), timestamp, assemblyPaths.Skip(1).ToArray());
            batch.Execute();
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

        private static CommandParsingException CreateParsingException(CommandLineApplication command, string message, int exitCode)
        {
            var exception = new CommandParsingException(command, message);
            exception.Data[ExitCodeKeyForExceptionData] = exitCode;
            return exception;
        }
    }
}