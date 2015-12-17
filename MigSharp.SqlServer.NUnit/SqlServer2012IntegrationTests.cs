using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2012")]
    public class SqlServer2012IntegrationTests : SqlServerIntegrationTests
    {
        protected override DbPlatform DbPlatform { get { return DbPlatform.SqlServer2012; } }
    }
}