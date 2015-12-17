using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2008")]
    public class SqlServer2008IntegrationTests : SqlServerIntegrationTests
    {
        protected override DbPlatform DbPlatform { get { return DbPlatform.SqlServer2008; } }
    }
}