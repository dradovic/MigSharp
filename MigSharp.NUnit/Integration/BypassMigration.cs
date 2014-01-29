using System.Data;

namespace MigSharp.NUnit.Integration
{
    internal class BypassMigration : IMigration
    {
        public const string TableName = "Bypass";

        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
              .WithPrimaryKeyColumn("Id", DbType.Int32);
        }
    }
}