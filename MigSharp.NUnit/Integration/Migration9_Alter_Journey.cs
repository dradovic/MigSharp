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
              .WithNotNullableColumn(Tables[0].Columns[0], DbType.Int32)
              .WithNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(128)
              .WithNotNullableColumn(UniqueColumn, DbType.Int32).Unique(UniqueColumnConstraint);

            // rename table
            db.Tables[TableNameInitial].Rename(Tables[0].Name);

            // add primary key constraint
            if (db.Context.ProviderMetadata.Platform != Platform.Teradata && 
                db.Context.ProviderMetadata.Platform != Platform.SQLite) // Teradata and SQLite do not support adding/dropping of PKs
            {
                db.Tables[Tables[0].Name].AddPrimaryKey()
                                         .OnColumn(Tables[0].Columns[0]);
            }

            // alter column to nullable
            if (db.Context.ProviderMetadata.Platform != Platform.SQLite) // SQLite does not support dropping of columns
            {
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[1]].AlterToNotNullable(DbType.String).OfSize(128);
            }

            // add column, alter it to not nullable right afterwards
            db.Tables[Tables[0].Name].AddNullableColumn(Tables[0].Columns[2], DbType.Double);
            if (db.Context.ProviderMetadata.Platform != Platform.SQLite) // SQLite does not support altering of columns
            {
                db.Tables[Tables[0].Name].Columns[Tables[0].Columns[2]].AlterToNotNullable(DbType.Double);
            }

            // add colum (through rename if provider supports it) and the drop again
            if (db.Context.ProviderMetadata.Platform == Platform.SqlServerCe ||
                db.Context.ProviderMetadata.Platform == Platform.SQLite ||
                db.Context.ProviderMetadata.Platform == Platform.MySql)
            {
                db.Tables[Tables[0].Name].AddNullableColumn(TempColumnRenamed, DbType.DateTime);
            }
            else
            {
                db.Tables[Tables[0].Name].AddNullableColumn(TempColumn, DbType.DateTime);
                db.Tables[Tables[0].Name].Columns[TempColumn].Rename(TempColumnRenamed);
            }
            if (db.Context.ProviderMetadata.Platform != Platform.SQLite) // SQLite does not support dropping of columns
            {
                db.Tables[Tables[0].Name].Columns[TempColumnRenamed].Drop();
            }

            // remove, add and remove again unique constraint
            db.Tables[Tables[0].Name].UniqueConstraints[UniqueColumnConstraint].Drop();
            db.Tables[Tables[0].Name].AddUniqueConstraint(UniqueColumnConstraint)
                                     .OnColumn(UniqueColumn);
            db.Tables[Tables[0].Name].UniqueConstraints[UniqueColumnConstraint].Drop();
            if (db.Context.ProviderMetadata.Platform != Platform.SQLite) // SQLite does not support dropping of columns
            {
                db.Tables[Tables[0].Name].Columns[UniqueColumn].Drop();
            }
            else
            {
                db.Tables[Tables[0].Name].Drop();
                db.CreateTable(Tables[0].Name)
                  .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
                  .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(128);
                db.Tables[Tables[0].Name].AddNullableColumn(Tables[0].Columns[2], DbType.Double);
            }

            // add index
            db.Tables[Tables[0].Name].AddIndex("My Index")
                                     .OnColumn(Tables[0].Columns[1])
                                     .OnColumn(Tables[0].Columns[2]);

            // insert test record
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"",""{2}"",""{3}"") VALUES ({4},'{5}',{6})",
                                     Tables[0].Name,
                                     Tables[0].Columns[0],
                                     Tables[0].Columns[1],
                                     Tables[0].Columns[2],
                                     Tables[0].Value(0, 0),
                                     Tables[0].Value(0, 1),
                                     Tables[0].Value(0, 2)));

            // remove index
            db.Tables[Tables[0].Name].Indexes["My Index"].Drop();
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig9", "Id", "Name", "Grade")
                            {
                                { 1, "Charlie", 5.5 },
                            }
                    };
            }
        }
    }
}