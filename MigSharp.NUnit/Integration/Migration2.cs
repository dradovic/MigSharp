using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(ModuleName = Module, Tag = Tag)]
    internal class Migration2 : IReversibleMigration, IIntegrationTestMigration
    {
        public const string Module = "Migration 2";
        public const string Tag = "Test Module/Tag support and more...";

        public void Up(IDatabase db)
        {
            if (db.Context.ProviderMetadata.Name == "a provider that does not exist")
            {
                db.Execute("bogus query which would fail");
            }

            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32);

            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" VALUES ({1})", Tables[0].Name, Tables[0].Value(0, 0)));
        }

        public void Down(IDatabase db)
        {
            // Teradata and SQLite do not allow droping of primary keys
            if (db.Context.ProviderMetadata.Name.Contains("Teradata") || db.Context.ProviderMetadata.Name == ProviderNames.SQLite)
            {
                // create new table and copy content from original table
                const string temporaryName = "tmprename";
                db.Tables[Tables[0].Name].Rename(temporaryName);
                db.CreateTable(Tables[0].Name)
                    .WithNotNullableColumn(Tables[0].Columns[0], DbType.Int32);
                string query = string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" SELECT * FROM {1}", Tables[0].Name, temporaryName);
                db.Execute(query);
                db.Tables[temporaryName].Drop();
            }
            else
            {
                db.Tables[Tables[0].Name].PrimaryKey().Drop(); // we could also directly drop the table but thus, the dropping of PK is tested as well
            }
            db.Tables[Tables[0].Name].Drop();
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Order Space", "Id Space")
                    {
                        1
                    }
                };
            }
        }
    }
}