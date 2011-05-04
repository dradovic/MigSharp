using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Adding and Dropping Indexes")]
    internal class Migration7 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32)
                .WithNotNullableColumn(ColumnNames[1], DbType.String).OfSize(128)
                .WithNotNullableColumn(ColumnNames[2], DbType.Double);

            db.Tables[TableName].AddIndex("My Index")
                .OnColumn(ColumnNames[1])
                .OnColumn(ColumnNames[2]);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"",""{2}"",""{3}"") VALUES ({4},'{5}',{6})",
                TableName,
                ColumnNames[0],
                ColumnNames[1],
                ColumnNames[2],
                ExpectedValues[0, 0],
                ExpectedValues[0, 1],
                ExpectedValues[0, 2]));

            db.Tables[TableName].Indexes["My Index"].Drop();
        }

        public string TableName { get { return "Mig7"; } }
        public string[] ColumnNames { get { return new[] { "Id", "Name", "Grade" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, "Charlie", 5.5 },
                };
            }
        }
    }
}