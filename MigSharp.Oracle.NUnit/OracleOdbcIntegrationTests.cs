using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.Oracle.NUnit
{
    [TestFixture, Category("OracleOdbc")]
    public class OracleOdbcIntegrationTests : IntegrationTestsBase
    {
        protected static string Server
        {
            get
            {
                const string server = "MIGSHARP_ORACLE_SERVER";
                return GetEnvironmentVariable(server);
            }
        }

        protected static string Password
        {
            get
            {
                const string password = "MIGSHARP_ORACLE_PASSWORD";
                return GetEnvironmentVariable(password);
            }
        }

        protected static string TableSpace
        {
            get
            {
                const string tableSpace = "MIGSHARP_ORACLE_TABLESPACE";
                return GetEnvironmentVariable(tableSpace);
            }
        }

        private string _testDbName = "MigSharp_TestDb2";
        protected internal string TestDbName { get { return _testDbName; } set { _testDbName = value; } }

        private string _user = "MigSharp_TestDb2";
        protected internal string User { get { return _user; } set { _user = value; } }

        private static string GetUniqueDbName()
        {
            //Oracle only allows 30 chars for database name and a guid is 38 so we use this shortened form
            //Oracle requires name to start with a letter no longer a true short guid as we use substring added timestamp to ensure unique and remove illegal chars
            return string.Format(CultureInfo.InvariantCulture, "A{0}{1}", Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_")
                                                                                 .Replace("+", "_"), DateTime.Now.Minute.ToString(CultureInfo.CurrentCulture) + DateTime.Now.Second + DateTime.Now.Millisecond);
        }

        [SetUp]
        public override void Setup()
        {
            Environment.SetEnvironmentVariable("ORA_NCHAR_LITERAL_REPLACE", "TRUE"); // required for N'...' literals (Migration23) (only works for the ODBC driver)

            TestDbName = GetUniqueDbName();
            User = TestDbName;
            base.Setup();

            try
            {
                DropDatabase(TestDbName);
            }
            catch (OdbcException)
            {
                Console.WriteLine("Just making sure database doesn't exist error");
            }
            CreateDatabase(TestDbName);
        }

        protected override DbDataAdapter GetDataAdapter(string tableName, string schemaName, out DbCommandBuilder builder)
        {
            var adapter = new OdbcDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\"", tableName), OdbcConnectionString);
            builder = new OdbcCommandBuilder(adapter);
            return adapter;
        }

        protected static string MasterConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "Driver={{Oracle in OraClient11g_home1}};Dbq={0};Uid={1};Pwd={2};", Server, "system", Password); } }

        protected string OdbcConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "Driver={{Oracle in OraClient11g_home1}};Dbq={0};Uid={1};Pwd={2};Option=256;", Server, User, Password); } }

        protected override string ConnectionString { get { return OdbcConnectionString; } }

        protected override DbPlatform DbPlatform { get { return new DbPlatform(Platform.Oracle, 10, Driver.Odbc); } }

        [TearDown]
        public override void Teardown()
        {
            base.Teardown();

            DropDatabase(TestDbName);
        }

        [Test]
        public override void TestMigration1UsingConsoleApp()
        {
            // we don't execute this test yet since the Migrate.exe
            // would require a config file that includes the definition of
            // the .Net Framework Data Provider
        }

        private static void CreateDatabase(string databaseName)
        {
            var query1 = @"CREATE USER " + databaseName + " IDENTIFIED BY " + Password + " DEFAULT TABLESPACE " + TableSpace + " QUOTA UNLIMITED ON " + TableSpace;
            var query = @"GRANT CONNECT, RESOURCE TO " + databaseName;

            using (OdbcConnection con = new OdbcConnection(MasterConnectionString))
            {
                con.Open();

                using (var com = new OdbcCommand(query1, con))
                {
                    com.ExecuteNonQuery();
                    com.CommandText = query;
                    com.ExecuteNonQuery();
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public int DropDatabase(string databaseName)
        {
            OdbcIntegrationTestHelper.CloseAllOdbcConnections();

            using (var con = new OdbcConnection(MasterConnectionString))
            {
                con.Open();

                using (var com = new OdbcCommand("DROP USER " + databaseName + " CASCADE", con))
                {
                    try
                    {
                        Console.WriteLine("Executing: {0}", com.CommandText);
                        return com.ExecuteNonQuery();
                    }
                    catch (OdbcException)
                    {
                        return -1;
                    }
                }
            }
        }
    }
}