using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Cascaded Delete")]
    internal class Migration22 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.CreateTable(Tables[0].Name, "Mig22PrimaryKey") // parent table
              .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
              .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(255);

            db.CreateTable(Tables[1].Name)
              .WithPrimaryKeyColumn(Tables[1].Columns[0], DbType.Int32).AsIdentity()
              .WithNotNullableColumn(Tables[1].Columns[1], DbType.Int32);

            db.Tables[Tables[1].Name].AddForeignKeyTo(Tables[0].Name, "Mig22ChildForeignKey")
                                     .Through(Tables[1].Columns[1], Tables[0].Columns[0])
                                     .CascadeOnDelete();

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], "Parent Row 1"));
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], "Parent Row 2"));
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[1].Name, Tables[1].Columns[1], 1));
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[1].Name, Tables[1].Columns[1], 2));
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"DELETE FROM ""{0}"" WHERE ""{1}"" = 1", Tables[0].Name, Tables[0].Columns[0])); // removing the parent row should delete its child rows
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig22", "Id", "Name")
                            {
                                { 2, "Parent Row 2" },
                            },
                        new ExpectedTable("Mig22Child", "Id", "ParentId")
                            {
                                { 2, 2 }
                            }
                    };
            }
        }

        public IEnumerable<string> ProvidersNotSupportingFeatureUnderTest
        {
            get
            {
                return new[]
                    {
                        ProviderNames.SQLite,
                        ProviderNames.Teradata,
                        ProviderNames.TeradataOdbc,
                    };
            }
        }
    }
}