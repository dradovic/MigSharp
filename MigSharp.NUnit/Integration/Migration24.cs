using System;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test boolean default value")]
    internal class Migration24 : IIntegrationTestMigration
    {
        private const bool DefaultValue = true;

        public void Up(IDatabase db)
        {
            string tableName = Tables[0].Name;

            db.CreateTable(tableName)
              .WithPrimaryKeyColumn("Id", DbType.Int32)
              .WithNotNullableColumn("Flag", DbType.Boolean).HavingDefault(DefaultValue);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES (1)", tableName, Tables[0].Columns[0])); // without 'Flag'
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES (2, 0)", tableName, Tables[0].Columns[0], Tables[0].Columns[1]));
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES (3, 1)", tableName, Tables[0].Columns[0], Tables[0].Columns[1]));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig24", "Id", "Flag")
                            {
                                { 1, AssertIs(true) },
                                { 2, AssertIs(false) },
                                { 3, AssertIs(true) },    
                            }
                    };
            }
        }

        private static Func<object, bool> AssertIs(bool expectedValue)
        {
            return value => Convert.ToBoolean(value, CultureInfo.InvariantCulture) == expectedValue;
        }
    }
}