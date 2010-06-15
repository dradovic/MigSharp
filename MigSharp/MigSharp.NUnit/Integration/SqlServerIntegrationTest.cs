using System.Data.SqlClient;
using System.Reflection;

using Microsoft.SqlServer.Management.Smo;

using MigSharp.Process;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [TestFixture, Category("SqlServer")]
    public class SqlServerIntegrationTest
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

        [Test]
        public void Test()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                InitialCatalog = TestDbName,
                IntegratedSecurity = true,
            };
            Migrator migrator = new Migrator(builder.ConnectionString, "System.Data.SqlClient");
            migrator.UpgradeAll(Assembly.GetExecutingAssembly());
            Assert.IsNotNull(_database.Tables[DbVersion.TableName], string.Format("The '{0}' table was not created.", DbVersion.TableName));
        }

        [TearDown]
        public void TearDown()
        {
            //_database.Drop(); // TODO: comment in
        }
    }
}