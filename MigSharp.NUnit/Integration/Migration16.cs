using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Renaming Column With Default Value and Adding New Column With Default Value Having the Previous Name")]
    internal class Migration16 : IIntegrationTestMigration
    {
        const string FirstDefaultValue = "Test";
        const int SecondDefaultValue = 747;

        public void Up(IDatabase db)
        {
            // renaming columns is not supported by SqlServerCe4 and SQLite
            bool renameColumnIsSupported = db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe4 && db.Context.ProviderMetadata.Name != ProviderNames.SQLite;

            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32)
                .WithNotNullableColumn(renameColumnIsSupported ? ColumnNames[2] : ColumnNames[1], DbType.String).OfSize(10).HavingDefault(FirstDefaultValue);

            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", TableName, ColumnNames[0], ExpectedValues[0, 0]));

            if (renameColumnIsSupported) 
            {
                db.Tables[TableName].Columns[ColumnNames[2]].Rename(ColumnNames[1]);
            }

            // add a new column with the same name like the previously renamed one to make sure that any associated db object were renamed, too
            db.Tables[TableName].AddNotNullableColumn(ColumnNames[2], DbType.Int32).HavingDefault(SecondDefaultValue);

            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", TableName, ColumnNames[0], ExpectedValues[1, 0]));
        }

        public string TableName { get { return "Mig16"; } }
        public string[] ColumnNames { get { return new[] { "Id", "First Renamed", "First" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, FirstDefaultValue, SecondDefaultValue },
                    { 2, FirstDefaultValue, SecondDefaultValue },
                };
            }
        }
    }
}