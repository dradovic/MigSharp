using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Odbc;
using System.Globalization;
using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.Teradata.NUnit
{
    [TestFixture, Category("TeradataOdbc")]
    public class TeradataOdbcIntegrationTests : IntegrationTestsBase
    {
        protected static string Server
        {
            get
            {
                const string server = "MIGSHARP_TERADATA_SERVER";
                return GetEnvironmentVariable(server);
            }
        }

        protected static string Password
        {
            get
            {
                const string password = "MIGSHARP_TERADATA_PASSWORD";
                return GetEnvironmentVariable(password);
            }
        }

        protected static string User
        {
            get
            {
                const string user = "MIGSHARP_TERADATA_USER";
                return GetEnvironmentVariable(user);
            }
        }

        private readonly string _databaseName = GetUniqueDbName();
        protected string DatabaseName { get { return _databaseName; } }


        private static string GetUniqueDbName()
        {
            //Teradata only allows 30 chars for databse name and a guid is 38 so we use this shortend form
            //Oracle requires name to start with a letter no longer a true short guid as we use substring addedtimestamp to ensure unique and remove illegal chars
            return string.Format(CultureInfo.InvariantCulture, "A{0}{1}", Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_")
                                                                                 .Replace("+", "_"), DateTime.Now.Minute.ToString(CultureInfo.CurrentCulture) + DateTime.Now.Second + DateTime.Now.Millisecond);
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            try
            {
                DropDatabase(_databaseName);
            }
            catch (OdbcException)
            {
                Console.WriteLine("Just making sure database doesn't exist");
            }
            CreateDatabase(_databaseName, null);
        }

        protected override DbDataAdapter GetDataAdapter(string tableName, string schemaName, out DbCommandBuilder builder)
        {
            var adapter = new OdbcDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\" ORDER BY 1", tableName), OdbcConnectionString);
            builder = new OdbcCommandBuilder(adapter);
            return adapter;
        }

        protected string OdbcConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "Driver={{Teradata}}; DBCName={0} ; Uid={1} ; Pwd={2} ; Database={3};USENATIVELOBSUPPORT=YES;", Server, User, Password, _databaseName); } }

        protected override string ConnectionString { get { return OdbcConnectionString; } }

        protected static string MasterConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "Driver={{Teradata}}; DBCName={0} ; Uid={1} ; Pwd={2} ; Database={3};", Server, User, Password, "dbc"); } }

        protected override DbPlatform DbPlatform { get { return new DbPlatform(Platform.Teradata, 12, Driver.Odbc); } }

        [TearDown]
        public override void Teardown()
        {
            base.Teardown();

            DropDatabase(_databaseName);
        }

        public static int CreateDatabase(string databaseName, Dictionary<string, string> options)
        {
            using (OdbcConnection con = new OdbcConnection(MasterConnectionString))
            {
                con.Open();

                string permSpace = "10e6";
                if (options != null && options.ContainsKey("perm"))
                    permSpace = options["perm"];

                return ExecuteNonQuery("CREATE DATABASE " + '"' + databaseName + '"' + " from dbc as PERM=" + permSpace, con);
            }
        }

        public static int DropDatabase(string databaseName)
        {
            OdbcIntegrationTestHelper.CloseAllOdbcConnections();

            using (var con = new OdbcConnection(MasterConnectionString))
            {
                con.Open();

                ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "SELECT SYSLIB.AbortSessions(1, '{0}', 0, 'Y', 'Y');", User), con);
                ExecuteNonQuery("DELETE DATABASE " + '"' + databaseName + '"', con);
                return ExecuteNonQuery("DROP DATABASE " + '"' + databaseName + '"', con);
            }
        }

        private static int ExecuteNonQuery(string commandText, OdbcConnection connection)
        {
            using (var command = new OdbcCommand(commandText, connection))
            {
                Console.WriteLine("Executing: '{0}'", command.CommandText);
                return command.ExecuteNonQuery();
            }
        }

        [Test]
        public override void TestMigrationWithinTransactionScopeComplete()
        {
            // we don't execute this test yet since
            // TransactionScope is not fully supported
            // by this provider
        }

        [Test]
        public override void TestMigrationWithinTransactionScopeRollback()
        {
            // we don't execute this test yet since
            // TransactionScope is not fully supported
            // by this provider
        }
    }
}