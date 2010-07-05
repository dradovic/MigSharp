using System.Data;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport]
    internal class Migration1 : IReversibleMigration
    {
        public const string CustomerTableName = "Customer";
        public static string[] ColumnNames = new[]
        {
            "Id",
        };

        public void Up(IDatabase db)
        {
            db.CreateTable(CustomerTableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32);
        }

        public void Down(IDatabase db)
        {
            db.Tables[CustomerTableName].Drop();
        }
    }
}