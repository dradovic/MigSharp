using System;
using System.Data;
using MigSharp.Providers;
using NUnit.Framework;

namespace MigSharp.NUnit.Providers
{
    [TestFixture, Category("smoke")]
    public class SqlScriptingHelperTests
    {
        [Test]
        public void TestWrappingOfFormatExceptions()
        {
            Assert.That(() => SqlScriptingHelper.ToSql("0", DbType.Boolean, false), Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Could not convert '0' of type System.String to a SQL expression of type Boolean."));
        }
    }
}