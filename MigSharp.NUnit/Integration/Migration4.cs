using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Identity")]
    internal class Migration4 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int64).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"")VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[0].Value(0, 1)));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig4", "Id", "Description")
                    {
                        { 1, "Test Row" },
                    }
                };
            }
        }
    }
}