using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Identity")]
    internal class Migration4 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int64).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.String);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"")VALUES ('{2}')", TableName, ColumnNames[1], ExpectedValues[0, 1]));
        }

        public string TableName { get { return "IdentityTable"; } }
        public string[] ColumnNames { get { return new[] { "Id", "Description" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, "Test Row" },
                };
            }
        }
    }
}