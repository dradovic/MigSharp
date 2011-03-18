using System;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Altering Columns")]
    internal class Migration5 : IIntegrationTestMigration
    {
        private const string Default3 = "First try's Default"; // note the single quote
        private const string SecondDefault3 = "Second try's Default"; // note the single quote

        public void Up(IDatabase db)
        {
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite) // SQLite does not support altering of columns
            {
                db.CreateTable(TableName)
                    .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                    .WithNotNullableColumn(ColumnNames[1], DbType.AnsiString).OfSize(2000)
                    .WithNotNullableColumn(ColumnNames[2], DbType.String).OfSize(2000)
                    .WithNotNullableColumn(ColumnNames[3], DbType.AnsiString).OfSize(2000);
            }
            else
            {
                db.CreateTable(TableName)
                    .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                    .WithNotNullableColumn(ColumnNames[1], DbType.String).OfSize(2000)
                    .WithNullableColumn(ColumnNames[2], DbType.String).OfSize(2000)
                    .WithNullableColumn(ColumnNames[3], DbType.String).OfSize(2000);
            }

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"", ""Second"", ""Third"") VALUES ('{1}', '{2}', '{3}')", TableName, ExpectedValues[0, 1], ExpectedValues[0, 2], ExpectedValues[0, 3]));
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[TableName].Columns[ColumnNames[1]].AlterToNotNullable(DbType.String).OfSize(2000); // changing the type from AnsiString to String should be possible without further problems (required by migration 7 of the Security Component)
                db.Tables[TableName].Columns[ColumnNames[2]].AlterToNullable(DbType.String).OfSize(2000); // changing the nullability but keeping the current datatype
                db.Tables[TableName].Columns[ColumnNames[3]].AlterToNullable(DbType.String).OfSize(2000); // changing the nullability *and* the datatype at the same time
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", TableName, ExpectedValues[1, 1])); // try to execute without second description and third description as they should allow null now
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}""='{2}' WHERE ""{1}"" IS NULL", TableName, ColumnNames[3], ExpectedValues[1, 3]));
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[TableName].Columns[ColumnNames[3]].AlterToNotNullable(DbType.String).OfSize(2000).HavingDefault(Default3);
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", TableName, ExpectedValues[2, 1])); // try again to execute without third description as it should have a default now
            if (db.Context.ProviderMetadata.Name == ProviderNames.SQLite)
            {
                // simulate the default
                db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}""='{2}' WHERE ""{1}"" IS NULL", TableName, ColumnNames[3], ((string)ExpectedValues[2, 3]).Replace("'", "''")));                
            }
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[TableName].Columns[ColumnNames[3]].AlterToNotNullable(DbType.String).OfSize(2000).HavingDefault(SecondDefault3); // change the default
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", TableName, ExpectedValues[3, 1])); // try again to execute without third description as it should have another default now
            if (db.Context.ProviderMetadata.Name == ProviderNames.SQLite)
            {
                // simulate the default
                db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}""='{2}' WHERE ""{1}"" IS NULL", TableName, ColumnNames[3], ((string)ExpectedValues[3, 3]).Replace("'", "''")));
            }
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[TableName].Columns[ColumnNames[3]].AlterToNullable(DbType.String).OfSize(2000); // remove the Default3 default value again
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", TableName, ExpectedValues[4, 1])); // try again to execute without third description, and this time it should be NULL
        }

        public string TableName { get { return "Mig5"; } }
        public string[] ColumnNames { get { return new[] { "Id", "First", "Second", "Third" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    //{ 1, "Test Row", "Unicodović", "Something" }, // FIXME: don, non LATIN-1 chars should work too
                    { 1, "Test Row", "Irgendöppis", "Something" },
                    { 2, "Only one desc", DBNull.Value, "New non-null value" },
                    { 3, "Another one", DBNull.Value, Default3 },
                    { 4, "Yet another one", DBNull.Value, SecondDefault3 },
                    { 5, "And yet another one", DBNull.Value, DBNull.Value },
                };
            }
        }
    }
}