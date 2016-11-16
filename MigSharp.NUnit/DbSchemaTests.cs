using System;
using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class DbSchemaTests
    {
        [Test, ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Could not find a provider for 'SqlServer, Version: 8, Driver: AdoNet'.")]
        public void TestSelectingNonExistingProvider()
        {
            DbSchema schema = new DbSchema("...", new DbPlatform(Platform.SqlServer, 8));
            Assert.IsNotNull(schema); // we should never hit this
        }

        [Test, ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Could not find a provider for 'SqlServer, Version: 8, Driver: Odbc'.")]
        public void TestSelectingNonExistingOdbcProvider()
        {
            DbSchema schema = new DbSchema("...", new DbPlatform(Platform.SqlServer, 8, Driver.Odbc));
            Assert.IsNotNull(schema); // we should never hit this
        }
    }
}