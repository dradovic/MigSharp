using NUnit.Framework;

namespace MigSharp.SqlServerCe.NUnit
{
    [TestFixture, Category("SqlServerCe4")]
    public class SqlServerCe4IntegrationTests : SqlServerCeIntegrationTestsBase
    {
        protected override DbPlatform DbPlatform { get { return DbPlatform.SqlServerCe4; } }

        protected override string CeVersion { get { return "4.0"; } }
    }
}