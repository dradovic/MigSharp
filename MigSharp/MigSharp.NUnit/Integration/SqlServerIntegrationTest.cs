using System.Data;
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
            //MigrationExportAttribute[] attributes = (MigrationExportAttribute[])typeof(Migration1).GetCustomAttributes(typeof(MigrationExportAttribute), false);
            migrator.UpgradeAll(Assembly.GetExecutingAssembly());
            
            // assert DbVersion table was created
            Table dbVersionTable = _database.Tables[DbVersion.TableName];
            Assert.IsNotNull(dbVersionTable, string.Format("The '{0}' table was not created.", DbVersion.TableName));
            Assert.AreEqual(3, dbVersionTable.Columns.Count);
            Assert.AreEqual("Timestamp", dbVersionTable.Columns[0].Name);
            Assert.AreEqual("Module", dbVersionTable.Columns[1].Name);
            Assert.AreEqual("Tag", dbVersionTable.Columns[2].Name);

            // assert Customer table was created
            Table customerTable = _database.Tables[Migration1.CustomerTableName];
            Assert.IsNotNull(customerTable, string.Format("The '{0}' table was not created.", Migration1.CustomerTableName));
            Assert.AreEqual(1, customerTable.Columns.Count);
            Assert.AreEqual(Migration1.ColumnNames[0], customerTable.Columns[0].Name);

            // assert DbVersion table has necessary entries
            DataSet dbVersionContent = _database.ExecuteWithResults(string.Format("SELECT * FROM [{0}]", DbVersion.TableName));
            Assert.AreEqual(1, dbVersionContent.Tables["Table"].Rows.Count, "The versioning table is missing entries.");
        }

        [TearDown]
        public void TearDown()
        {
            //_database.Drop(); // TODO: comment in
        }
    }
}