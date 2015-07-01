using System.Data;
using System.Globalization;
using System.Linq;

using MigSharp.Providers;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Identity")]
    internal class Migration4 : IIntegrationTestMigration
    {
        public static readonly ExpectedTables ExpectedTables = new ExpectedTables();

        public void Up(IDatabase db)
        {
            ExpectedTables.Clear();

            foreach (SupportsAttribute support in IntegrationTestContext.SupportsAttributes
                .Where(s => !IntegrationTestContext.IsScripting || s.IsScriptable)
                .Where(s => s.CanBeUsedAsIdentity))
            {
                if (support.CanBeUsedAsPrimaryKey)
                {
                    CreateTableWithOneRecord(support, db, true);
                }
                if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite &&
                    db.Context.ProviderMetadata.Name != ProviderNames.MySqlExperimental) // MySql and SQLite do not support identity on non-primary key columns
                {
                    CreateTableWithOneRecord(support, db, false);
                }
            }
        }

        private static void CreateTableWithOneRecord(SupportsAttribute support, IDatabase db, bool onPrimaryKey)
        {
            // add expectation
            const string expectedContent = "Something";
            var expectedTable = new ExpectedTable(string.Format(CultureInfo.InvariantCulture, "Mig4 {0}Identity {1}", onPrimaryKey ? "PK-" : string.Empty, support.DbType),
                "Id", "Content");
            ExpectedTables.Add(expectedTable);
            expectedTable.Add(1, expectedContent);

            // create table
            var table = db.CreateTable(expectedTable.Name);
            var identityColumn = onPrimaryKey
                                     ? table.WithPrimaryKeyColumn("Id", support.DbType).AsIdentity()
                                     : table.WithNotNullableColumn("Id", support.DbType).AsIdentity();
            if (support.MaximumSize > 0)
            {
                identityColumn.OfSize(support.MaximumSize, support.MaximumScale > 0 ? support.MaximumScale : (int?)null);
            }
            table.WithNotNullableColumn("Content", DbType.String).OfSize(255);

            // insert one record to see if Identity works
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", expectedTable.Name, "Content", expectedContent));
        }

        public ExpectedTables Tables { get { return ExpectedTables; } }
    }
}