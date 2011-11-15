using NUnit.Framework;

namespace MigSharp.SqlServerCe.NUnit
{
    [TestFixture, Category("SqlServerCe3.5")]
    public class SqlServerCe35IntegrationTests : SqlServerCeIntegrationTestsBase
    {
        protected override string ProviderName { get { return ProviderNames.SqlServerCe4; } }

        protected override string CeVersion { get { return "3.5"; } }
    }
}