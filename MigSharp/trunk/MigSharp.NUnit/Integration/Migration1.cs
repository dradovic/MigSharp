using System.Data;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport]
    internal class Migration1 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32);
        }

        public string TableName { get { return "Mig1"; } }
        public string[] ColumnNames { get { return new[] { "Id" }; } }
        public object[,] ExpectedValues { get { return new object[0,1]; } }
    }
}