using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FakeItEasy;
using JetBrains.Annotations;
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
        public void TestValidCommandLineArguments(string commandLine, string expectedConnectionString, DbPlatform expectedDbPlatform, string expectedAssemblyPath, string[] expectedAdditionalAssemblyPaths,
                                                  long expectedTimestamp, SourceLevels expectedTraceLevels, Action<MigrationOptions> checkOptions)
        {
            string connectionString;
            DbPlatform dbPlatform;
            string assemblyPath;
            string[] additionalAssemblyPaths;
            long timestamp;
            SourceLevels traceLevels;
            MigrationOptions options = Parse(commandLine, out connectionString, out dbPlatform, out assemblyPath, out additionalAssemblyPaths, out timestamp, out traceLevels);

            Assert.AreEqual(expectedConnectionString, connectionString, "The connectionString does not match.");
            Assert.IsTrue(expectedDbPlatform.Matches(dbPlatform), "The providerSpecifier does not match.");
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
        [UsedImplicitly]
        private IEnumerable<TestCaseData> GetValidCommandLineCases()
        {
            yield return new TestCaseData("Migrate.exe qa some.dll",
                                          QaConnectionString, new DbPlatform(CommandLineOptions.DefaultPlatform, 1000, Driver.AdoNet), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                    {
                                                  AssertModulesAreSelected(o, "Module1", "Module2");
                                                  Assert.AreEqual(ScriptingMode.ExecuteOnly, o.ScriptingOptions.Mode);
                                                  CollectionAssert.IsEmpty(o.SupportedPlatforms, "There are no means to govern the supported providers from the console.");
                                                  Assert.AreEqual(MigrationOptions.DefaultVersioningTableName, o.VersioningTableName);
                                              }))
                .SetDescription("Default");

            yield return new TestCaseData(@"Migrate.exe qa foo.dll ""C:\Temp\My Migrations\bar.dll""",
                                          QaConnectionString, new DbPlatform(CommandLineOptions.DefaultPlatform, 1000, Driver.AdoNet), "foo.dll", new[] { @"C:\Temp\My Migrations\bar.dll" }, long.MaxValue, SourceLevels.Warning, null)
                .SetDescription("Additional assembly path containing a space");

            yield return new TestCaseData("Migrate.exe qa some.dll -platform Oracle",
                                          QaConnectionString, new DbPlatform(Platform.Oracle, DbPlatform.MaximumMajorVersion), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, null)
                .SetDescription("-platform");

            yield return new TestCaseData("Migrate.exe qa some.dll -platform Oracle -version 10",
                                          QaConnectionString, new DbPlatform(Platform.Oracle, 10), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, null)
                .SetDescription("-platform -version");

            yield return new TestCaseData("Migrate.exe qa some.dll -platform Oracle -version 10 -driver Odbc",
                                          QaConnectionString, new DbPlatform(Platform.Oracle, 10, Driver.Odbc), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, null)
                .SetDescription("-platform -version -driver");

            yield return new TestCaseData("Migrate.exe qa some.dll -module Module2",
                                          QaConnectionString, new DbPlatform(CommandLineOptions.DefaultPlatform, 1000, Driver.AdoNet), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                                              {
                                                  AssertModulesAreNotSelected(o, "Module1");
                                                  AssertModulesAreSelected(o, "Module2");
                                              }))
                .SetDescription("-module");

            yield return new TestCaseData("Migrate.exe qa some.dll -to 12",
                                          QaConnectionString, new DbPlatform(CommandLineOptions.DefaultPlatform, 1000, Driver.AdoNet), "some.dll", new string[] { }, 12, SourceLevels.Warning, null)
                .SetDescription("-to");

            yield return new TestCaseData(@"Migrate.exe qa some.dll -scriptTo D:\Temp",
                                          QaConnectionString, new DbPlatform(CommandLineOptions.DefaultPlatform, 1000, Driver.AdoNet), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                                              {
                                                  Assert.AreEqual(ScriptingMode.ScriptAndExecute, o.ScriptingOptions.Mode);
                                                  Assert.AreEqual(@"D:\Temp", o.ScriptingOptions.TargetDirectory.FullName);
                                              }))
                .SetDescription("-scriptTo");

            yield return new TestCaseData(@"Migrate.exe qa some.dll -scriptTo ""My Documents"" -scriptOnly",
                                          QaConnectionString, new DbPlatform(CommandLineOptions.DefaultPlatform, 1000, Driver.AdoNet), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
                                              {
                                                  Assert.AreEqual(ScriptingMode.ScriptOnly, o.ScriptingOptions.Mode);
                                                  Assert.AreEqual("My Documents", o.ScriptingOptions.TargetDirectory.Name);
                                              }))
                .SetDescription("-scriptTo");

            yield return new TestCaseData(@"Migrate.exe qa some.dll -versioningTable ""My Versioning Table""",
                                          QaConnectionString, new DbPlatform(CommandLineOptions.DefaultPlatform, 1000, Driver.AdoNet), "some.dll", new string[] { }, long.MaxValue, SourceLevels.Warning, (Action<MigrationOptions>)(o =>
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

        private static void AssertModulesAreSelected(MigrationOptions options, params string[] moduleNames)
        {
            foreach (string moduleName in moduleNames)
            {
                var metadata = A.Fake<IMigrationMetadata>();
                A.CallTo(() => metadata.ModuleName).Returns(moduleName);
                Assert.IsTrue(options.MigrationSelector(metadata), "Module '{0}' should be selected.", moduleName);
            }
        }

        private static void AssertModulesAreNotSelected(MigrationOptions options, params string[] moduleNames)
        {
            foreach (string moduleName in moduleNames)
            {
                var metadata = A.Fake<IMigrationMetadata>();
                A.CallTo(() => metadata.ModuleName).Returns(moduleName);
                Assert.IsFalse(options.MigrationSelector(metadata), "Module '{0}' should not be selected.", moduleName);
            }
        }

        [Test, TestCaseSource("GetInvalidCommandLineCases")]
        public void TestInvalidCommandLineArguments(string commandLine, int expectedExitCode)
        {
            try
            {
                string connectionString;
                DbPlatform dbPlatform;
                string assemblyPath;
                string[] additionalAssemblyPaths;
                long timestamp;
                SourceLevels traceLevels;
                Parse(commandLine, out connectionString, out dbPlatform, out assemblyPath, out additionalAssemblyPaths, out timestamp, out traceLevels);
                Assert.Fail("Exception was not thrown.");
            }
            catch (InvalidCommandLineArgumentException x)
            {
                Assert.IsNotEmpty(x.Message);
                Assert.AreEqual(expectedExitCode, x.ExitCode, "Wrong ExitCode.");
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [UsedImplicitly]
        private IEnumerable<TestCaseData> GetInvalidCommandLineCases()
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

        private static MigrationOptions Parse(string commandLine, out string connectionString, out DbPlatform dbPlatform, out string assemblyPath, out string[] additionalAssemblyPaths, out long timestamp, out SourceLevels traceLevels)
        {
            CommandLineOptions commandLineOptions;
            CommandLineParser parser;
            Assert.IsFalse(Program.DisplayHelp(commandLine, out commandLineOptions, out parser));

            var connectionStrings = new ConnectionStringSettingsCollection
                {
                    new ConnectionStringSettings(QaTarget, QaConnectionString)
                };
            return Program.ParseCommandLineArguments(commandLineOptions, parser, connectionStrings,
                                                     out connectionString, out dbPlatform, out assemblyPath, out additionalAssemblyPaths, out timestamp, out traceLevels);
        }
    }
}