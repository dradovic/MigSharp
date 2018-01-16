using System;
using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class DbSchemaTests
    {
        [Test]
        public void TestSelectingNonExistingProvider()
        {
            Assert.That(() => new DbSchema("...", new DbPlatform(Platform.SqlServer, 8)), Throws.TypeOf<NotSupportedException>().With.Message.EqualTo("Could not find a provider for 'SqlServer, Version: 8, Driver: AdoNet'."));
        }

        [Test]
        public void TestSelectingNonExistingOdbcProvider()
        {
            Assert.That(() => new DbSchema("...", new DbPlatform(Platform.SqlServer, 8, Driver.Odbc)), Throws.TypeOf<NotSupportedException>().With.Message.EqualTo("Could not find a provider for 'SqlServer, Version: 8, Driver: Odbc'."));
        }
    }
}