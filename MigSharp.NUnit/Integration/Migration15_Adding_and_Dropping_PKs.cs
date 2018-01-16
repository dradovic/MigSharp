using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Adding and Dropping of Primary Keys")]
    internal class Migration15 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.CreateTable(Tables[0].Name)
              .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
              .WithNotNullableColumn(Tables[0].Columns[1], DbType.Int32)
              .WithNotNullableColumn(Tables[0].Columns[2], DbType.Int32);

            // insert first record
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" VALUES({1}, {2}, {3})",
                                     Tables[0].Name,
                                     Tables[0].Value(0, 0),
                                     Tables[0].Value(0, 1),
                                     Tables[0].Value(0, 2)));

            // add and drop primary keys
            db.Tables[Tables[0].Name].PrimaryKey().Drop();

            db.Tables[Tables[0].Name].AddPrimaryKey("Test Mig15")
                                     .OnColumn(Tables[0].Columns[1]);
            db.Tables[Tables[0].Name].PrimaryKey("Test Mig15").Drop();

            db.Tables[Tables[0].Name].AddPrimaryKey()
                                     .OnColumn(Tables[0].Columns[2]);
            db.Tables[Tables[0].Name].PrimaryKey().Drop();

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" VALUES({1}, {2}, {3})",
                                     Tables[0].Name,
                                     Tables[0].Value(0, 0),
                                     Tables[0].Value(0, 1),
                                     Tables[0].Value(0, 2)));
        }

        public ExpectedTables Tables
        {
            get
            {
                var expectedTables = new ExpectedTables
                    {
                        new ExpectedTable("Mig15", "Col1", "Col2", "Col3")
                            {
                                { 1, 2, 3 },
                                { 1, 2, 3 },
                            }
                    };
                return expectedTables;
            }
        }

        public IEnumerable<Platform> PlatformsNotSupportingFeatureUnderTest
        {
            get
            {
                return new[]
                    {
                        Platform.SQLite,
                    };
            }
        }
    }
}