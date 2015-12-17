using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
// ReSharper disable InconsistentNaming
    public abstract class SQLiteIntegrationTestsBase : IntegrationTestsBase
// ReSharper restore InconsistentNaming
    {
        protected override DbPlatform DbPlatform { get { return DbPlatform.SQLite3; } }

        [Test]
        public override void TestMigration1UsingConsoleApp()
        {
            // we don't execute this test yet since the Migrate.exe
            // would require a config file that includes the definition of
            // the .Net Framework Data Provider
        }
    }
}