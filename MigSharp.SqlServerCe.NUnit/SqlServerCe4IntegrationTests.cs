using NUnit.Framework;

namespace MigSharp.SqlServerCe.NUnit
{
    [TestFixture, Category("SqlServerCe4")]
    public class SqlServerCe4IntegrationTests : SqlServerCeIntegrationTestsBase
    {
        protected override string ProviderName { get { return ProviderNames.SqlServerCe4; } }

        protected override string CeVersion { get { return "4.0"; } }
    }
}