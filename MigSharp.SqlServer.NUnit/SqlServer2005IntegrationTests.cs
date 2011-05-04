using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2005")]
    public class SqlServer2005IntegrationTests : SqlServerIntegrationTests
    {
        protected override string ProviderName { get { return ProviderNames.SqlServer2005; } }
    }
}