using System;
using System.Data;
using MigSharp.Providers;
using NUnit.Framework;

namespace MigSharp.NUnit.Providers
{
    [TestFixture, Category("smoke")]
    public class SqlScriptingHelperTests
    {
        [Test, ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Could not convert '0' of type System.String to a SQL expression of type Boolean.")]
        public void TestWrappingOfFormatExceptions()
        {
            string sql = SqlScriptingHelper.ToSql("0", DbType.Boolean, false);
            Assert.IsNull(sql); // we should never reach this point
        }
    }
}