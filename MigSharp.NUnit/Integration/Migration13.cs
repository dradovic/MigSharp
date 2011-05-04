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
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.Int32); // FK to 'other'

            const string otherId = "Id";
            const string otherName = "Name";
            db.CreateTable(Other)
                .WithPrimaryKeyColumn(otherId, DbType.Int32).AsIdentity()
                .WithNotNullableColumn(otherName, DbType.String).OfSize(255);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Other, otherName, "Not Referenced"));
            db.Execute(GetDeleteStatementForOther()); // removing the row from Other should not be a problem since it is not referenced
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Other, otherName, "Referenced"));

            db.Tables[TableName].AddForeignKeyTo(Other)
                .Through(ColumnNames[1], otherId);

            // insert a row that references a row from Other
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", TableName, ColumnNames[1], ExpectedValues[0, 1]));

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
                        catch (DbException)
                        {
                            // this is expected
                        }
                    });
            }
        }

        private static string GetDeleteStatementForOther()
        {
            return string.Format(CultureInfo.InvariantCulture, @"DELETE FROM ""{0}""", Other);
        }

        public string TableName { get { return "Mig13"; } }
        public string[] ColumnNames { get { return new[] { "Id", "OtherId" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, 2 },
                };
            }
        }
    }
}