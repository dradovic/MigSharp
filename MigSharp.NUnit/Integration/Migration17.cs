using System.Collections.Generic;
using System.Data;
using System.Globalization;

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
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(50);
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[0].Value(0, 1)));

            if (db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe4 &&
                db.Context.ProviderMetadata.Name != ProviderNames.SQLite &&
                !db.Context.ProviderMetadata.Name.Contains("Teradata"))
            {
                db.Tables[Tables[0].Name].PrimaryKey().Rename("PK_" + NewTableName);
            }
            else if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4)
            {
                // this code is actually not required for the test but we still execute it because it is what the recommendation
                // of the SqlServerCe4 is in the NotSupportedException for the primary key renaming
                db.Tables[Tables[0].Name].PrimaryKey().Drop();
                db.Tables[Tables[0].Name].AddPrimaryKey("PK_" + NewTableName)
                    .OnColumn(Tables[0].Columns[0]);
            }
            db.Tables[Tables[0].Name].Rename(NewTableName);

            // create a new table that has the same name as the previously renamed table and also has an identity column to check
            // if associated db objects that are managed by the provider itself were renamed along with the table 
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(50);
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[0].Value(0, 1)));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig17", "Id Column", "Name")
                    {
                        { 1, "Test" },
                    },
                    new ExpectedTable(NewTableName, "Id Column", "Name")
                    {
                        { 1, "Test" },
                    }
                };
            }
        }
    }
}