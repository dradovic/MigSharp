﻿using System.Data;
using System.Globalization;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Renaming Primary Keys & Renaming Table Having an Identity Column and Creating New Table the Previous Name and an Identity Column")]
    internal class Migration17 : IIntegrationTestMigration
    {
        private const string NewTableName = "Mig17 Renamed";

        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(255);
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[1].Value(0, 1)));
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[1].Value(1, 1)));

            if (db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe4 &&
                db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe35 &&
                db.Context.ProviderMetadata.Name != ProviderNames.SQLite &&
                !db.Context.ProviderMetadata.Name.Contains("Teradata"))
            {
                db.Tables[Tables[0].Name].PrimaryKey().Rename("PK_" + NewTableName);
            }
            else if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4 || db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe35)
            {
                // this code is actually not required for the test but we still execute it because it is what the recommendation
                // of the SqlServerCe4 and SqlServerCe35 is in the NotSupportedException for the primary key renaming
                db.Tables[Tables[0].Name].PrimaryKey().Drop();
                db.Tables[Tables[0].Name].AddPrimaryKey("PK_" + NewTableName)
                    .OnColumn(Tables[0].Columns[0]);
            }
            db.Tables[Tables[0].Name].Rename(NewTableName);
            // insert another row into the renamed table to verify that the Identity is still working
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", NewTableName, Tables[1].Columns[1], Tables[1].Value(2, 1)));

            // create a new table that has the same name as the previously renamed table and also has an identity column to check
            // if associated db objects that are managed by the provider itself were renamed along with the table 
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(255);
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[0].Value(0, 1)));
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[0].Value(1, 1)));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig17", "Id Column", "Name")
                    {
                        { 1, "New" },
                        { 2, "New 2" },
                    },
                    new ExpectedTable(NewTableName, "Id Column", "Name")
                    {
                        { 1, "Original" },
                        { 2, "Original 2" },
                        { 3, "This record was inserted after renaming the table" },
                    }
                };
            }
        }
    }
}