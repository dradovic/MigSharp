using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.MySql.NUnit 
{
    public abstract class MySqlIntegrationTestsBase : IntegrationTestsBase 
    {
        protected override string ProviderName { get { return ProviderNames.MySql; } }

        [Test]
        public override void TestMigration1UsingConsoleApp() {
            // we don't execute this test yet since the Migrate.exe
            // would require a config file that includes the definition of
            // the .Net Framework Data Provider
        }
    }
}