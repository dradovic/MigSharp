using System.Data;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(2010, 6, 15, 21, 55, 12)]
    internal class Migration1 : IMigration
    {
        #region Implementation of IMigration

        public void Up(IDatabase db)
        {
            db.CreateTable("Customer")
                .WithPrimaryKeyColumn("Id", DbType.Int32);
        }

        #endregion
    }
}