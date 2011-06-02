using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test HavingCurrentDateTimeAsDefault")]
    internal class Migration6 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.DateTime).HavingCurrentDateTimeAsDefault();

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Value(0, 0)));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig6", "Id", "CurrentDateTime")
                    {
                        { 1, new Func<object, bool>(v => Math.Abs((DateTime.Now - (DateTime)v).TotalHours) <= 24) },
                    }
                };
            }
        }
    }
}