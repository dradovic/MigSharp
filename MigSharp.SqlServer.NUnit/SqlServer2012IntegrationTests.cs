using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2012")]
    public class SqlServer2012IntegrationTests : SqlServerIntegrationTests
    {
        protected override string ProviderName { get { return ProviderNames.SqlServer2012; } }
    }
}