using System.Data;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(2010, 6, 17, 12, 50, 21)]
    internal class Migration2 : IMigration
    {
        public const string OrderTableName = "Order";
        public static string[] ColumnNames = new[]
        {
            "Id",
        };

        public void Up(IDatabase db)
        {
            db.CreateTable(OrderTableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32);
        }
    }
}