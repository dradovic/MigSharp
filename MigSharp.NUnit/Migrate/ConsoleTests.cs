using System.Diagnostics;

using MigSharp.Migrate;

using NUnit.Framework;

namespace MigSharp.NUnit.Migrate
{
    [TestFixture, Category("smoke")]
    public class ConsoleTests
    {
        private const int SuccessExitCode = 0;

        [Test]
        public void ProvidingNoArgumentsShouldFail()
        {
            int exitCode = MigrateProcess.Execute(null);
            Assert.AreEqual(Program.InvalidArgumentsExitCode, exitCode);
        }

        [Test]
        public void ProvidingNotExistingTargetShouldFail()
        {
            const string arguments = "target-xxx some.dll";
            int exitCode = MigrateProcess.Execute(arguments);
            Assert.AreEqual(Program.InvalidTargetExitCode, exitCode);
        }

        [Test]
        public void ProvidingNotExistingAssemblyShouldFail()
        {
            const string arguments = "qa some.dll";
            int exitCode = MigrateProcess.Execute(arguments);
            Assert.AreEqual(Program.FailedMigrationExitCode, exitCode);
        }

        [Test]
        public void ProvidingOnlyHelpArgumentShouldSucceed()
        {
            const string arguments = "-help";
            int exitCode = MigrateProcess.Execute(arguments);
            Assert.AreEqual(SuccessExitCode, exitCode);
        }
    }
}