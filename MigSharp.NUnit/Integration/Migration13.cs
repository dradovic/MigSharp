using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

using MigSharp.Core;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Foreign Keys")]
    internal class Migration13 : IIntegrationTestMigration
    {
        private const string Other = "Other";

        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.Int32); // FK to 'other'

            const string otherId = "Id";
            const string otherName = "Name";
            db.CreateTable(Other)
                .WithPrimaryKeyColumn(otherId, DbType.Int32).AsIdentity()
                .WithNotNullableColumn(otherName, DbType.String).OfSize(255);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Other, otherName, "Not Referenced"));
            db.Execute(GetDeleteStatementForOther()); // removing the row from Other should not be a problem since it is not referenced
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Other, otherName, "Referenced"));

            db.Tables[Tables[0].Name].AddForeignKeyTo(Other)
                .Through(Tables[0].Columns[1], otherId);

            // insert a row that references a row from Other
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[0].Value(0, 1)));

            // removing the row from Other should fail as it is referenced by the foreign key
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite) // Mig# does not support SQLite foreign keys (see comments in SQLiteProvider.AddForeignKey)
            {
                db.Execute(context =>
                    {
                        IDbCommand command = context.Connection.CreateCommand();
                        command.Transaction = context.Transaction;
                        command.CommandText = GetDeleteStatementForOther();
                        Log.Verbose(LogCategory.Sql, command.CommandText);
                        try
                        {
                            command.ExecuteNonQuery();
                            Assert.Fail("The previous query should have failed.");
                        }
                        catch (Exception ex)
                        {
                            // this is expected
                            if (db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe35 && !(ex is DbException))
                                throw;
                            // for the case of a SqlServer35 the SqlCeException is not derived from DbException
                        }
                    });
            }
        }

        private static string GetDeleteStatementForOther()
        {
            return string.Format(CultureInfo.InvariantCulture, @"DELETE FROM ""{0}""", Other);
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig13", "Id", "OtherId")
                    {
                        { 1, 2 },
                    }
                };
            }
        }
    }
}