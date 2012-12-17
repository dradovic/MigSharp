using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
    public abstract class SQLiteIntegrationTestsBase : IntegrationTestsBase
    {
        protected override string ProviderName { get { return ProviderNames.SQLite; } }

        [Test]
        public override void TestMigration1UsingConsoleApp()
        {
            // we don't execute this test yet since the Migrate.exe
            // would require a config file that includes the definition of
            // the .Net Framework Data Provider
        }
    }
}