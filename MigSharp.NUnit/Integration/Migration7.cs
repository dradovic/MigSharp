using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Adding and Dropping Indexes")]
    internal class Migration7 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(128)
                .WithNotNullableColumn(Tables[0].Columns[2], DbType.Double);

            db.Tables[Tables[0].Name].AddIndex("My Index")
                .OnColumn(Tables[0].Columns[1])
                .OnColumn(Tables[0].Columns[2]);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"",""{2}"",""{3}"") VALUES ({4},'{5}',{6})",
                Tables[0].Name,
                Tables[0].Columns[0],
                Tables[0].Columns[1],
                Tables[0].Columns[2],
                Tables[0].Value(0, 0),
                Tables[0].Value(0, 1),
                Tables[0].Value(0, 2)));

            db.Tables[Tables[0].Name].Indexes["My Index"].Drop();
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig7", "Id", "Name", "Grade")
                    {
                        { 1, "Charlie", 5.5 },
                    }
                };
            }
        }
    }
}