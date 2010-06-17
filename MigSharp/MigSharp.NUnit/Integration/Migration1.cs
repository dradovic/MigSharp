using System.Data;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(2010, 6, 15, 21, 55, 12)]
    internal class Migration1 : IMigration
    {
        public const string CustomerTableName = "Customer";
        public static string[] ColumnNames = new[]
        {
            "Id",
        };

        #region Implementation of IMigration

        public void Up(IDatabase db)
        {
            db.CreateTable(CustomerTableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32);
        }

        #endregion
    }
}