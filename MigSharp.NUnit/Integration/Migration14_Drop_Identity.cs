using System;
using System.Data;
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
            MySqlHelper.ActivateStrictMode(db);

            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String);

            db.Execute(GetInsertStatement((string)Tables[0].Value(0, 1)));

            // Note: the following statement does *not* the identity constraint. Doing so is very difficult. For example, see: http://stackoverflow.com/questions/702745/sql-server-how-to-drop-identity-from-a-column
            //db.Tables[TableName].Columns[ColumnNames[0]].AlterToNotNullable(DbType.Int32);

            db.Tables[Tables[0].Name].Drop(); // make sure, TRIGGERS and SEQUENCES are dropped as well

            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String);

            // inserting another row without specifying the Id value should fail as the Identity constraint is removed
            if (db.Context.ProviderMetadata.Platform != Platform.SQLite) // SQLite automatically generates identity columns for PKs
            {
                db.Execute(context =>
                    {
                        IDbCommand command = context.CreateCommand();
                        command.CommandText = GetInsertStatement((string)Tables[0].Value(0, 1));
                        Log.Verbose(LogCategory.Sql, command.CommandText);
                        try
                        {
                            command.ExecuteNonQuery();
                            Assert.Fail("The previous query should have failed.");
                        }
                        catch (Exception x)
                        {
                            if (!x.IsDbException())
                            {
                                throw;
                            }
                        }
                    });
            }

            db.Execute(GetInsertStatement((int)Tables[0].Value(0, 0), (string)Tables[0].Value(0, 1)));

            db.Tables[Tables[0].Name].Drop(); // make sure, TRIGGERS and SEQUENCES are dropped as well

            // recreating the table with the identity constraint again might reveal undropped TRIGGERS or SEQUENCES
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String);

            db.Execute(GetInsertStatement((string)Tables[0].Value(0, 1)));
        }

        private string GetInsertStatement(int id, string name)
        {
            return string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, '{4}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Columns[1], id, name);
        }

        private string GetInsertStatement(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], name);
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig14", "Id", "Name")
                    {
                        { 1, "Name1" },
                    }
                };
            }
        }
    }
}