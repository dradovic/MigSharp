using NUnit.Framework;

namespace Migrate.NUnit
{
    [TestFixture, Category("smoke")]
    public class ConsoleTests
    {
        private const int SuccessExitCode = 0;

        [Test]
        public void ProvidingNoArgumentsShouldFail()
        {
            int exitCode = MigrateProcess.Execute(null);
#if NET462
            Assert.AreEqual(MigSharp.Migrate.Program.InvalidArgumentsExitCode, exitCode);
#elif NETCOREAPP2_0
            Assert.AreEqual(Cli.Program.InvalidArgumentsExitCode, exitCode);
#endif
        }

        [Test]
        public void ProvidingNotExistingTargetShouldFail()
        {
            const string arguments = "target-xxx some.dll";
            int exitCode = MigrateProcess.Execute(arguments);
#if NET462
            Assert.AreEqual(MigSharp.Migrate.Program.InvalidTargetExitCode, exitCode);
#elif NETCOREAPP2_0
            Assert.AreEqual(Cli.Program.InvalidTargetExitCode, exitCode);
#endif
        }

        [Test]
        public void ProvidingNotExistingAssemblyShouldFail()
        {
            const string arguments = "qa some.dll";
            int exitCode = MigrateProcess.Execute(arguments);
#if NET462
            Assert.AreEqual(MigSharp.Migrate.Program.FailedMigrationExitCode, exitCode);
#elif NETCOREAPP2_0
            Assert.AreEqual(Cli.Program.FailedMigrationExitCode, exitCode);
#endif
        }

        [Test]
        public void ProvidingOnlyHelpArgumentShouldSucceed()
        {
            const string arguments = "--help";
            int exitCode = MigrateProcess.Execute(arguments);
            Assert.AreEqual(SuccessExitCode, exitCode);
        }
    }
}
