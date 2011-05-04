using System;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test HavingCurrentDateTimeAsDefault")]
    internal class Migration6 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32)
                .WithNotNullableColumn(ColumnNames[1], DbType.DateTime).HavingCurrentDateTimeAsDefault();

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", TableName, ColumnNames[0], ExpectedValues[0, 0]));
        }

        public string TableName { get { return "Mig6"; } }
        public string[] ColumnNames { get { return new[] { "Id", "CurrentDateTime" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, new Func<object, bool>(v => Math.Abs((DateTime.Now - (DateTime)v).TotalHours) <= 24) },
                };
            }
        }
    }
}