using System.Data;
using System.Data.SqlClient;

using Microsoft.SqlServer.Management.Smo;

using MigSharp.NUnit.Integration;

using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer")]
    public class SqlServerIntegrationTest : IntegrationTestsBase
    {
        private const string Server = "localhost";
        private const string TestDbName = "MigSharp_TestDb";

        private Database _database;

        [SetUp]
        public void SetUp()
        {
            var server = new Server(Server);

            var database = server.Databases[TestDbName];
            if (database != null)
            {
                database.Drop();
            }

            _database = new Database(server, TestDbName);
            _database.Create();
        }

        protected override DataTable GetTable(string tableName)
        {
            return _database.ExecuteWithResults(string.Format("SELECT * FROM [{0}]", tableName)).Tables["Table"];
        }

        protected override string GetConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                InitialCatalog = TestDbName,
                IntegratedSecurity = true,
            };
            return builder.ConnectionString;
        }

        [TearDown]
        public void TearDown()
        {
            SqlConnection.ClearAllPools();
            //_database.Drop(); // TODO: comment in
        }
    }
}