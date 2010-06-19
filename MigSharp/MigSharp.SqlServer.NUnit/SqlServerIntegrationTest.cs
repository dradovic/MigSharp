using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

using Microsoft.SqlServer.Management.Smo;

using MigSharp.NUnit.Integration;
using MigSharp.Process;

using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
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
        public void TestMigration1()
        {
            Migrator migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            DateTime timestamp1 = GetTimestamp(typeof(Migration1));
            migrator.UpgradeUntil(typeof(Migration1).Assembly, timestamp1);
            
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
            DataTable dbVersion = _database.ExecuteWithResults(string.Format("SELECT * FROM [{0}]", DbVersion.TableName)).Tables["Table"];
            Assert.AreEqual(1, dbVersion.Rows.Count, "The versioning table is missing entries.");
            Assert.AreEqual(timestamp1, dbVersion.Rows[0][0], "The timestamp of Migration1 is wrong.");
            Assert.AreEqual(string.Empty, dbVersion.Rows[0][1], "The module of Migration1 is wrong.");
            Assert.AreEqual(DBNull.Value, dbVersion.Rows[0][2], "The tag of Migration1 is wrong.");
        }

        [Test]
        public void TestMigration1SuccededByMigration2()
        {
            Migrator migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            DateTime timestamp1 = GetTimestamp(typeof(Migration1));
            DateTime timestamp2 = GetTimestamp(typeof(Migration2));
            Assembly assemblyContainingMigrations = typeof(Migration1).Assembly;
            migrator.UpgradeUntil(assemblyContainingMigrations, timestamp1);

            migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            migrator.UpgradeAll(assemblyContainingMigrations);

            // assert Order table was created and contains all entries
            Table orderTable = _database.Tables[Migration2.OrderTableName];
            Assert.IsNotNull(orderTable, string.Format("The '{0}' table was not created.", Migration2.OrderTableName));
            Assert.AreEqual(1, orderTable.Columns.Count);
            Assert.AreEqual(Migration2.ColumnNames[0], orderTable.Columns[0].Name);
            DataTable orderContent = _database.ExecuteWithResults(string.Format("SELECT * FROM [{0}]", Migration2.OrderTableName)).Tables["Table"];
            Assert.AreEqual(1, orderContent.Rows.Count, "The order table does not contain all expected entries.");
            Assert.AreEqual(Migration2.FirstId, orderContent.Rows[0][Migration2.ColumnNames[0]]);

            // assert DbVersion table has necessary entries
            DataTable dbVersion = _database.ExecuteWithResults(string.Format("SELECT * FROM [{0}]", DbVersion.TableName)).Tables["Table"];
            Assert.AreEqual(2, dbVersion.Rows.Count, "The versioning table is missing entries.");
            Assert.AreEqual(timestamp2, dbVersion.Rows[1][0], "The timestamp of Migration2 is wrong.");
            Assert.AreEqual(Migration2.Module, dbVersion.Rows[1][1], "The module of Migration2 is wrong.");
            Assert.AreEqual(Migration2.Tag, dbVersion.Rows[1][2], "The tag of Migration2 is wrong.");
        }

        private static DateTime GetTimestamp(Type migration)
        {
            MigrationExportAttribute[] attributes = (MigrationExportAttribute[])migration.GetCustomAttributes(typeof(MigrationExportAttribute), false);
            return new DateTime(attributes[0].Year, attributes[0].Month, attributes[0].Day, attributes[0].Hour, attributes[0].Minute, attributes[0].Second);
        }

        private static string GetConnectionString()
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