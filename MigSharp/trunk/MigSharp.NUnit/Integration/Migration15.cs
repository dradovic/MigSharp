using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Adding and Dropping of Primary Keys")]
    internal class Migration15 : IIntegrationTestMigration
    {
        private static bool _onlyExpectOneRecord;

        public void Up(IDatabase db)
        {
            if (!db.Context.ProviderMetadata.Name.Contains("Teradata") && db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.CreateTable(TableName)
                    .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32)
                    .WithNotNullableColumn(ColumnNames[1], DbType.Int32)
                    .WithNotNullableColumn(ColumnNames[2], DbType.Int32);
            }
            else
            {
                // Teradata and SQLite do not allow adding/dropping primary keys
                db.CreateTable(TableName)
                    .WithNotNullableColumn(ColumnNames[0], DbType.Int32)
                    .WithNotNullableColumn(ColumnNames[1], DbType.Int32)
                    .WithNotNullableColumn(ColumnNames[2], DbType.Int32);
            }

            // insert first record
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" VALUES({1}, {2}, {3})",
                TableName,
                ExpectedValues[0, 0],
                ExpectedValues[0, 1],
                ExpectedValues[0, 2]));

            // add and drop primary keys
            if (!db.Context.ProviderMetadata.Name.Contains("Teradata") && db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                db.Tables[TableName].PrimaryKey().Drop();

                db.Tables[TableName].AddPrimaryKey("Test_Mig_15")
                    .OnColumn(ColumnNames[1]);
                db.Tables[TableName].PrimaryKey("Test_Mig_15").Drop();

                db.Tables[TableName].AddPrimaryKey()
                    .OnColumn(ColumnNames[2]);
                db.Tables[TableName].PrimaryKey().Drop();
            }

            // there should be no primary key now, so adding the same values should be ok
            if (db.Context.ProviderMetadata.Name != ProviderNames.TeradataOdbc) // the Teradata *ODBC* driver auto-creates *unique* primary indexes which would lead to a "Duplicate row error"
            {
                db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" VALUES({1}, {2}, {3})",
                    TableName,
                    ExpectedValues[0, 0],
                    ExpectedValues[0, 1],
                    ExpectedValues[0, 2]));
                _onlyExpectOneRecord = false;
            }
            else
            {
                _onlyExpectOneRecord = true;
            }
        }

        public string TableName { get { return "Mig15"; } }
        public string[] ColumnNames { get { return new[] { "Col1", "Col2", "Col3" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                if (_onlyExpectOneRecord)
                {
                    return new object[,] { { 1, 2, 3 } };
                }
                return new object[,]
                {
                    { 1, 2, 3 },
                    { 1, 2, 3 },
                };
            }
        }
    }
}