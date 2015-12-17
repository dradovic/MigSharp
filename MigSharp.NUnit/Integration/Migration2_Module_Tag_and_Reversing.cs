using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(ModuleName = Module, Tag = Tag)]
    internal class Migration2 : IReversibleMigration, IIntegrationTestMigration
    {
        public const string Module = "Migration 2";
        public const string Tag = "Test Module/Tag support and undoing migrations (down-stepping)";

        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32);

            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" VALUES ({1})", Tables[0].Name, Tables[0].Value(0, 0)));
        }

        public void Down(IDatabase db)
        {
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