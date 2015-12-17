using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Globalization;
using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2005Odbc")]
    public class SqlServer2005OdbcIntegrationTests : SqlServerIntegrationTests
    {
        protected override DbDataAdapter GetDataAdapter(string tableName, string schemaName, out DbCommandBuilder builder)
        {
            var adapter = new OdbcDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[{1}]", schemaName ?? "dbo", tableName), ConnectionString);
            builder = new OdbcCommandBuilder(adapter);
            return adapter;
        }

        protected override DbPlatform DbPlatform { get { return new DbPlatform(Platform.SqlServer, 9, Driver.Odbc); } }

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