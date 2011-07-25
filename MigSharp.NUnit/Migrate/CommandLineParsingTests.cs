using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using MigSharp.Migrate;
using MigSharp.Migrate.Util;
using MigSharp.Process;

using NUnit.Framework;

namespace MigSharp.NUnit.Migrate
{
    [TestFixture, Category("smoke")]
    public class CommandLineParsingTests
    {
        private const string QaTarget = "qa";
        private const string QaConnectionString = "connection string for qa environment";

        [Test, TestCaseSource("GetValidCommandLineCases")]
        public void TestValidCommandLineArguments(string commandLine, string expectedConnectionString, string expectedProviderName, string expectedAssemblyPath, string[] expectedAdditionalAssemblyPaths,
            long expectedTimestamp, SourceLevels expectedTraceLevels, Action<MigrationOptions> checkOptions)
        {
            string connectionString;
            string providerName;
            string assemblyPath;
            string[] additionalAssemblyPaths;
            long timestamp;
            SourceLevels traceLevels;
            MigrationOptions options = Parse(commandLine, out connectionString, out providerName, out assemblyPath, out additionalAssemblyPaths, out timestamp, out traceLevels);

            Assert.AreEqual(expectedConnectionString, connectionString, "The connectionString does not match.");
            Assert.AreEqual(expectedProviderName, providerName, "The providerName does not match.");
            Assert.AreEqual(expectedAssemblyPath, assemblyPath, "The assemblyPath does not match.");
            CollectionAssert.AreEquivalent(expectedAdditionalAssemblyPaths, additionalAssemblyPaths, "The additionalAssemblyPaths do not match.");
            Assert.AreEqual(expectedTimestamp, timestamp, "The timestamp does not match.");
            Assert.AreEqual(expectedTraceLevels, traceLevels, "The traceLevel does not match.");
            if (checkOptions != null)
            {
                checkOptions(options); // execute further check on the MigrationOptions
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
// ReSharper disable UnusedMember.Local
        private IEnumerable<TestCaseData> GetValidCommandLineCases()
// ReSharper restore UnusedMember.Local
        {
            yield return new TestCaseData("Migrate.exe qa some.dll",
                QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                    {
                        Assert.AreEqual(2, Array.FindAll(new[] { "Module1", "Module2" }, o.ModuleSelector).Length, "All modules should be selected.");
                        Assert.AreEqual(ScriptingMode.ExecuteOnly, o.ScriptingOptions.Mode);
                        CollectionAssert.AreEquivalent(new[] { CommandLineOptions.DefaultProvider }, o.SupportedProviders.Names, "Only the selected provider should be supported as there are no means to govern the supported providers from the console.");
                        Assert.AreEqual(MigrationOptions.DefaultVersioningTableName, o.VersioningTableName);
                    }))
                .SetDescription("Default");

            yield return new TestCaseData(@"Migrate.exe qa foo.dll ""C:\Temp\My Migrations\bar.dll""",
                QaConnectionString, CommandLineOptions.DefaultProvider, "foo.dll", new[] { @"C:\Temp\My Migrations\bar.dll" }, long.MaxValue, SourceLevels.Warning, null)
                .SetDescription("Additional assembly path containing a space");

            yield return new TestCaseData("Migrate.exe qa some.dll -provider Oracle",
                QaConnectionString, ProviderNames.Oracle, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, null)
                .SetDescription("-provider");

            yield return new TestCaseData("Migrate.exe qa some.dll -module Module2",
                QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                    {
                        // ReSharper disable ConvertToLambdaExpression
                        Assert.AreEqual(1, Array.FindAll(new[] { "Module1", "Module2" }, o.ModuleSelector).Length, "Only one module should be selected.");
                        // ReSharper restore ConvertToLambdaExpression
                    }))
                .SetDescription("-module");

            yield return new TestCaseData("Migrate.exe qa some.dll -to 12",
                QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, 12, SourceLevels.Warning, null)
                .SetDescription("-to");

            yield return new TestCaseData(@"Migrate.exe qa some.dll -scriptTo D:\Temp",
                QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                    {
                        Assert.AreEqual(ScriptingMode.ScriptAndExecute, o.ScriptingOptions.Mode);
                        Assert.AreEqual(@"D:\Temp", o.ScriptingOptions.TargetDirectory.FullName);
                    }))
                .SetDescription("-scriptTo");

            yield return new TestCaseData(@"Migrate.exe qa some.dll -scriptTo ""My Documents"" -scriptOnly",
                QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                    {
                        Assert.AreEqual(ScriptingMode.ScriptOnly, o.ScriptingOptions.Mode);
                        Assert.AreEqual("My Documents", o.ScriptingOptions.TargetDirectory.Name);
                    }))
                .SetDescription("-scriptTo");

            yield return new TestCaseData(@"Migrate.exe qa some.dll -support Oracle;Teradata;;SQLite",
                QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                    {
                        // ReSharper disable ConvertToLambdaExpression
                        CollectionAssert.AreEquivalent(new[] { CommandLineOptions.DefaultProvider, "Oracle", "Teradata", "SQLite" }, o.SupportedProviders.Names.ToList(), "The names of supported providers do not match.");
                        // ReSharper restore ConvertToLambdaExpression
                    }))
                .SetDescription("-support");

            yield return new TestCaseData(@"Migrate.exe qa some.dll -versioningTable ""My Versioning Table""",
                QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                    {
                        // ReSharper disable ConvertToLambdaExpression
                        Assert.AreEqual("My Versioning Table", o.VersioningTableName);
                        // ReSharper restore ConvertToLambdaExpression
                    }))
                .SetDescription("-versioningTable");

            //yield return new TestCaseData("Migrate.exe qa some.dll -traceLevel Error",
            //    QaConnectionString, CommandLineOptions.DefaultProvider, "some.dll", new string[] { }, long.MaxValue, SourceLevels.Error, null)
            //    .SetDescription("-traceLevel");
        }

        [Test, TestCaseSource("GetInvalidCommandLineCases")]
        public void TestInvalidCommandLineArguments(string commandLine, int expectedExitCode)
        {
            try
            {
                string connectionString;
                string providerName;
                string assemblyPath;
                string[] additionalAssemblyPaths;
                long timestamp;
                SourceLevels traceLevels;
                Parse(commandLine, out connectionString, out providerName, out assemblyPath, out additionalAssemblyPaths, out timestamp, out traceLevels);
                Assert.Fail("Exception was not thrown.");
            }
            catch (InvalidCommandLineArgumentException x)
            {
                Assert.IsNotEmpty(x.Message);
                Assert.AreEqual(expectedExitCode, x.ExitCode, "Wrong ExitCode.");
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
// ReSharper disable UnusedMember.Local
        private IEnumerable<TestCaseData> GetInvalidCommandLineCases()
// ReSharper restore UnusedMember.Local
        {
            yield return new TestCaseData("Migrate.exe", Program.InvalidArgumentsExitCode)
                .SetDescription("No arguments");
            yield return new TestCaseData("Migrate.exe qa some.dll -xxx xxx", Program.InvalidArgumentsExitCode)
                .SetDescription("Unrecognized switch");
            yield return new TestCaseData("Migrate.exe target-xxx", Program.InvalidArgumentsExitCode)
                .SetDescription("Only target");
            yield return new TestCaseData("Migrate.exe target-xxx some.dll", Program.InvalidTargetExitCode)
                .SetDescription("Missing target");
            yield return new TestCaseData("Migrate.exe qa some.dll -to xxx", Program.InvalidArgumentsExitCode)
                .SetDescription("Inparsable -to");
            yield return new TestCaseData("Migrate.exe qa some.dll -traceLevel xxx", Program.InvalidArgumentsExitCode)
                .SetDescription("Inparsable -traceLevel");
            yield return new TestCaseData("Migrate.exe qa some.dll -scriptOnly", Program.InvalidArgumentsExitCode)
                .SetDescription("Only -scriptOnly");
        }

        private static MigrationOptions Parse(string commandLine, out string connectionString, out string providerName, out string assemblyPath, out string[] additionalAssemblyPaths, out long timestamp, out SourceLevels traceLevels)
        {
            CommandLineOptions commandLineOptions;
            CommandLineParser parser;
            Assert.IsFalse(Program.DisplayHelp(commandLine, out commandLineOptions, out parser));

            var connectionStrings = new ConnectionStringSettingsCollection
            {
                new ConnectionStringSettings(QaTarget, QaConnectionString)
            };
            return Program.ParseCommandLineArguments(commandLineOptions, parser, connectionStrings,
                out connectionString, out providerName, out assemblyPath, out additionalAssemblyPaths, out timestamp, out traceLevels);
        }
    }
}