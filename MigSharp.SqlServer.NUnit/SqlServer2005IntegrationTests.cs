using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2005")]
    public class SqlServer2005IntegrationTests : SqlServerIntegrationTests
    {
        protected override DbPlatform DbPlatform { get { return DbPlatform.SqlServer2005; } }
    }
}