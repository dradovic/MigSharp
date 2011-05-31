using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Renaming Primary Keys & Renaming Table Having an Identity Column and Creating New Table the Previous Name and an Identity Column")]
    internal class Migration17 : IIntegrationTestMigration
    {
        private const string NewTableName = "Mig17 Renamed";

        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.String).OfSize(50);
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", TableName, ColumnNames[1], ExpectedValues[0, 1]));

            if (db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe4 &&
                db.Context.ProviderMetadata.Name != ProviderNames.SQLite &&
                !db.Context.ProviderMetadata.Name.Contains("Teradata"))
            {
                db.Tables[TableName].PrimaryKey().Rename("PK_" + NewTableName);
            }
            else if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4)
            {
                // this code is actually not required for the test but we still execute it because it is what the recommendation
                // of the SqlServerCe4 is in the NotSupportedException for the primary key renaming
                db.Tables[TableName].PrimaryKey().Drop();
                db.Tables[TableName].AddPrimaryKey("PK_" + NewTableName)
                    .OnColumn(ColumnNames[0]);
            }
            db.Tables[TableName].Rename(NewTableName);

            // create a new table that has the same name as the previously renamed table and also has an identity column to check
            // if associated db objects that are managed by the provider itself were renamed along with the table 
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.String).OfSize(50);
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", TableName, ColumnNames[1], ExpectedValues[0, 1]));
        }

        public string TableName { get { return "Mig17"; } }
        public string[] ColumnNames { get { return new[] { "Id", "Name" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, "Test" },
                };
            }
        }
    }
}