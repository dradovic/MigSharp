using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Starting with empty table and alter")]
    internal class Migration9 : IIntegrationTestMigration
    {
        private const string TableNameInitial = "ChoseBadTableName";

        private const string TempColumn = "dummy";
        private const string TempColumnRenamed = "dummy-renamed";

        private const string UniqueColumn = "BusinessKey";
        private const string UniqueColumnConstraint = "BusinessKeyConstraint";

        public void Up(IDatabase db)
        {
            db.CreateTable(TableNameInitial)
                .WithNotNullableColumn(ColumnNames[0], DbType.Int32)
                .WithNullableColumn(ColumnNames[1], DbType.String).OfSize(128)
                .WithNotNullableColumn(UniqueColumn, DbType.Int32).Unique(UniqueColumnConstraint);

            // rename table
            db.Tables[TableNameInitial].Rename(TableName);

            // add primary key constraint
            if (!db.Context.ProviderMetadata.Name.Contains("Teradata")) // Teradata does not support adding/dropping of PKs
            {
                db.Tables[TableName].AddPrimaryKey()
                    .OnColumn(ColumnNames[0]);
            }

            // alter column to nullable
            db.Tables[TableName].Columns[ColumnNames[1]].AlterToNotNullable(DbType.String).OfSize(128);

            // add column, alter it to not nullable right afterwards
            db.Tables[TableName].AddNullableColumn(ColumnNames[2], DbType.Double);

            db.Tables[TableName].Columns[ColumnNames[2]].AlterToNotNullable(DbType.Double);

            // add colum (through rename if provider supports it) and the drop again
            if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4)
            {
                db.Tables[TableName].AddNullableColumn(TempColumnRenamed, DbType.DateTime);
            }
            else
            {
                db.Tables[TableName].AddNullableColumn(TempColumn, DbType.DateTime);
                db.Tables[TableName].Columns[TempColumn].Rename(TempColumnRenamed);
            }
            db.Tables[TableName].Columns[TempColumnRenamed].Drop();

            // remove, add and remove again unique constraint
            db.Tables[TableName].UniqueConstraints[UniqueColumnConstraint].Drop();
            db.Tables[TableName].AddUniqueConstraint(UniqueColumnConstraint)
                .OnColumn(UniqueColumn);
            db.Tables[TableName].UniqueConstraints[UniqueColumnConstraint].Drop();
            db.Tables[TableName].Columns[UniqueColumn].Drop();

            // add index
            db.Tables[TableName].AddIndex("My Index")
                .OnColumn(ColumnNames[1])
                .OnColumn(ColumnNames[2]);

            // insert test record
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"",""{2}"",""{3}"") VALUES ({4},'{5}',{6})",
                TableName,
                ColumnNames[0],
                ColumnNames[1],
                ColumnNames[2],
                ExpectedValues[0, 0],
                ExpectedValues[0, 1],
                ExpectedValues[0, 2]));

            // remove index
            db.Tables[TableName].Indexes["My Index"].Drop();
        }

        public string TableName { get { return "Mig9"; } }
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