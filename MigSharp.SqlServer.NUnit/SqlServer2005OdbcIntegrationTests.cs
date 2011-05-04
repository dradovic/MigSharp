using System;
using System.Data.Odbc;
using System.Globalization;

using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2005Odbc")]
    public class SqlServer2005OdbcIntegrationTests : SqlServerIntegrationTests
    {
        protected override string ProviderName { get { return ProviderNames.SqlServer2005Odbc; } }

        protected override string ConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "Driver={{SQL Native Client}};Server={0};Database={1};Trusted_Connection=yes", Server, TestDbName); } }

        public override void Teardown()
        {
            OdbcConnection.ReleaseObjectPool();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            base.Teardown();
        }
    }
}