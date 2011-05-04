using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2008")]
    public class SqlServer2008IntegrationTests : SqlServerIntegrationTests
    {
        protected override string ProviderName { get { return ProviderNames.SqlServer2008; } }
    }
}