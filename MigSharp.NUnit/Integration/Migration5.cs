using System;
using System.Collections.Generic;
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
                db.CreateTable(Tables[0].Name)
                    .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                    .WithNotNullableColumn(Tables[0].Columns[1], DbType.AnsiString).OfSize(2000)
                    .WithNotNullableColumn(Tables[0].Columns[2], DbType.String).OfSize(2000)
                    .WithNotNullableColumn(Tables[0].Columns[3], DbType.AnsiString).OfSize(2000);
            }
            else
            {
                db.CreateTable(Tables[0].Name)
                    .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                    .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(2000)
                    .WithNullableColumn(Tables[0].Columns[2], DbType.String).OfSize(2000)
                    .WithNullableColumn(Tables[0].Columns[3], DbType.String).OfSize(2000);
            }

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"", ""Second"", ""Third"") VALUES ('{1}', '{2}', '{3}')", Tables[0].Name, Tables[0].Value(0, 1), Tables[0].Value(0, 2), Tables[0].Value(0, 3)));
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[1]].AlterToNotNullable(DbType.String).OfSize(2000); // changing the type from AnsiString to String should be possible without further problems (required by migration 7 of the Security Component)
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[2]].AlterToNullable(DbType.String).OfSize(2000); // changing the nullability but keeping the current datatype
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[3]].AlterToNullable(DbType.String).OfSize(2000); // changing the nullability *and* the datatype at the same time
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", Tables[0].Name, Tables[0].Value(1, 1))); // try to execute without second description and third description as they should allow null now
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}""='{2}' WHERE ""{1}"" IS NULL", Tables[0].Name, Tables[0].Columns[3], Tables[0].Value(1, 3)));
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[3]].AlterToNotNullable(DbType.String).OfSize(2000).HavingDefault(Default3);
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", Tables[0].Name, Tables[0].Value(2, 1))); // try again to execute without third description as it should have a default now
            if (db.Context.ProviderMetadata.Name == ProviderNames.SQLite)
            {
                // simulate the default
                db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}""='{2}' WHERE ""{1}"" IS NULL", Tables[0].Name, Tables[0].Columns[3], ((string)Tables[0].Value(2, 3)).Replace("'", "''")));
            }
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[3]].AlterToNotNullable(DbType.String).OfSize(2000).HavingDefault(SecondDefault3); // change the default
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", Tables[0].Name, Tables[0].Value(3, 1))); // try again to execute without third description as it should have another default now
            if (db.Context.ProviderMetadata.Name == ProviderNames.SQLite)
            {
                // simulate the default
                db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}""='{2}' WHERE ""{1}"" IS NULL", Tables[0].Name, Tables[0].Columns[3], ((string)Tables[0].Value(3, 3)).Replace("'", "''")));
            }
            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[3]].AlterToNullable(DbType.String).OfSize(2000); // remove the Default3 default value again
            }
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""First"") VALUES ('{1}')", Tables[0].Name, Tables[0].Value(4, 1))); // try again to execute without third description, and this time it should be NULL
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig5", "Id", "First", "Second", "Third")
                    {
                        //{ 1, "Test Row", "Unicodović", "Something" }, // FIXME: don, non LATIN-1 chars should work too
                        { 1, "Test Row", "Irgendöppis", "Something" },
                        { 2, "Only one desc", DBNull.Value, "New non-null value" },
                        { 3, "Another one", DBNull.Value, Default3 },
                        { 4, "Yet another one", DBNull.Value, SecondDefault3 },
                        { 5, "And yet another one", DBNull.Value, DBNull.Value },
                    }
                };
            }
        }
    }
}