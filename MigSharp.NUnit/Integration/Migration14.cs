using System.Data;
using System.Data.Common;
using System.Globalization;

using MigSharp.Core;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test removing Identity constraint")] // some providers use SEQUENCEs and TRIGGERs which need to be disposed when not needed anymore
    internal class Migration14 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.String);

            db.Execute(GetInsertStatement((string)ExpectedValues[0, 1]));

            // Note: the following statement does *not* the identity constraint. Doing so is very difficult. For example, see: http://stackoverflow.com/questions/702745/sql-server-how-to-drop-identity-from-a-column
            //db.Tables[TableName].Columns[ColumnNames[0]].AlterToNotNullable(DbType.Int32);

            db.Tables[TableName].Drop(); // make sure, TRIGGERS and SEQUENCES are dropped as well

            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32)
                .WithNotNullableColumn(ColumnNames[1], DbType.String);

            // inserting another row without specifying the Id value should fail as the Identity constraint is removed
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite) // SQLite automatically generates identity columns for PKs
            {
                db.Execute(context =>
                    {
                        IDbCommand command = context.Connection.CreateCommand();
                        command.Transaction = context.Transaction;
                        command.CommandText = GetInsertStatement((string)ExpectedValues[0, 1]);
                        Log.Verbose(LogCategory.Sql, command.CommandText);
                        try
                        {
                            command.ExecuteNonQuery();
                            Assert.Fail("The previous query should have failed.");
                        }
                        catch (DbException)
                        {
                            // this is expected
                        }
                    });
            }

            db.Execute(GetInsertStatement((int)ExpectedValues[0, 0], (string)ExpectedValues[0, 1]));

            db.Tables[TableName].Drop(); // make sure, TRIGGERS and SEQUENCES are dropped as well

            // recreating the table with the identity constraint again might reveal undropped TRIGGERS or SEQUENCES
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.String);

            db.Execute(GetInsertStatement((string)ExpectedValues[0, 1]));
        }

        private string GetInsertStatement(int id, string name)
        {
            return string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, '{4}')", TableName, ColumnNames[0], ColumnNames[1], id, name);            
        }

        private string GetInsertStatement(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", TableName, ColumnNames[1], name);
        }

        public string TableName { get { return "Mig14"; } }
        public string[] ColumnNames { get { return new[] { "Id", "Name" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, "Name1" },
                };
            }
        }
    }
}