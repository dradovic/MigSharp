using System;
using System.Data;
using System.Reflection;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    public abstract class IntegrationTestsBase
    {
        [Test]
        public void TestMigration1()
        {
            Options.VersioningTableName = "My Versioning Table"; // test overriding the default versioning table name

            Migrator migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            DateTime timestamp1 = GetTimestamp(typeof(Migration1));
            migrator.MigrateTo(typeof(Migration1).Assembly, timestamp1);

            // assert Versioning table was created
            DataTable dbVersionTable = GetTable(Options.VersioningTableName);
            Assert.IsNotNull(dbVersionTable, string.Format("The '{0}' table was not created.", Options.VersioningTableName));
            Assert.AreEqual(3, dbVersionTable.Columns.Count);
            Assert.AreEqual("Timestamp", dbVersionTable.Columns[0].ColumnName);
            Assert.AreEqual("Module", dbVersionTable.Columns[1].ColumnName);
            Assert.AreEqual("Tag", dbVersionTable.Columns[2].ColumnName);

            // assert Customer table was created
            DataTable customerTable = GetTable(Migration1.CustomerTableName);
            Assert.IsNotNull(customerTable, string.Format("The '{0}' table was not created.", Migration1.CustomerTableName));
            Assert.AreEqual(1, customerTable.Columns.Count);
            Assert.AreEqual(Migration1.ColumnNames[0], customerTable.Columns[0].ColumnName);

            // assert Versioning table has necessary entries
            Assert.AreEqual(1, dbVersionTable.Rows.Count, "The versioning table is missing entries.");
            Assert.AreEqual(timestamp1, dbVersionTable.Rows[0][0], "The timestamp of Migration1 is wrong.");
            Assert.AreEqual(string.Empty, dbVersionTable.Rows[0][1], "The module of Migration1 is wrong.");
            Assert.AreEqual(DBNull.Value, dbVersionTable.Rows[0][2], "The tag of Migration1 is wrong.");
        }

        [Test]
        public void TestMigration1SuccededByMigration2()
        {
            Migrator migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            DateTime timestamp1 = GetTimestamp(typeof(Migration1));
            DateTime timestamp2 = GetTimestamp(typeof(Migration2));
            Assembly assemblyContainingMigrations = typeof(Migration1).Assembly;
            migrator.MigrateTo(assemblyContainingMigrations, timestamp1);

            migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            migrator.MigrateAll(assemblyContainingMigrations);

            // assert Order table was created and contains all entries
            DataTable orderTable = GetTable(Migration2.OrderTableName);
            Assert.IsNotNull(orderTable, string.Format("The '{0}' table was not created.", Migration2.OrderTableName));
            Assert.AreEqual(1, orderTable.Columns.Count);
            Assert.AreEqual(Migration2.ColumnNames[0], orderTable.Columns[0].ColumnName);
            Assert.AreEqual(1, orderTable.Rows.Count, "The order table does not contain all expected entries.");
            Assert.AreEqual(Migration2.FirstId, orderTable.Rows[0][Migration2.ColumnNames[0]], "The order table does not contain the expected Id value.");

            // assert Versioning table has necessary entries
            DataTable dbVersionTable = GetTable(Options.VersioningTableName);
            Assert.AreEqual(2, dbVersionTable.Rows.Count, "The versioning table is missing entries.");
            Assert.AreEqual(timestamp2, dbVersionTable.Rows[1][0], "The timestamp of Migration2 is wrong.");
            Assert.AreEqual(Migration2.Module, dbVersionTable.Rows[1][1], "The module of Migration2 is wrong.");
            Assert.AreEqual(Migration2.Tag, dbVersionTable.Rows[1][2], "The tag of Migration2 is wrong.");
        }

        [Test]
        public void TestUndoingMigration2()
        {
            Migrator migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            Assembly assemblyContainingMigrations = typeof(Migration1).Assembly;
            migrator.MigrateAll(assemblyContainingMigrations);

            migrator = new Migrator(GetConnectionString(), "System.Data.SqlClient");
            DateTime timestamp1 = GetTimestamp(typeof(Migration1));
            migrator.MigrateTo(assemblyContainingMigrations, timestamp1);

            // assert order table was dropped
            DataTable orderTable = GetTable(Migration2.OrderTableName);
            Assert.IsNull(orderTable, "The order table was not dropped.");

            // assert Versioning table has only necessary entries
            DataTable dbVersionTable = GetTable(Options.VersioningTableName);
            Assert.AreEqual(1, dbVersionTable.Rows.Count, "The versioning table is missing entries or has too much entries.");
            Assert.AreEqual(timestamp1, dbVersionTable.Rows[0][0], "The timestamp of Migration1 is wrong.");
            Assert.AreEqual(string.Empty, dbVersionTable.Rows[0][1], "The module of Migration1 is wrong.");
            Assert.AreEqual(DBNull.Value, dbVersionTable.Rows[0][2], "The tag of Migration1 is wrong.");
        }

        private static DateTime GetTimestamp(Type migration)
        {
            MigrationExportAttribute[] attributes = (MigrationExportAttribute[])migration.GetCustomAttributes(typeof(MigrationExportAttribute), false);
            return new DateTime(attributes[0].Year, attributes[0].Month, attributes[0].Day, attributes[0].Hour, attributes[0].Minute, attributes[0].Second);
        }

        /// <summary>
        /// Gets the content of the specified table or null if it does not exist.
        /// </summary>
        protected abstract DataTable GetTable(string tableName);

        protected abstract string GetConnectionString();
    }
}