using System;
using System.Data;
using System.Globalization;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test HavingCurrentDateTimeAsDefault and HavingCurrentUtcDateTimeAsDefault")]
    internal class Migration6 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.DateTime).HavingCurrentDateTimeAsDefault()
                .WithNotNullableColumn(Tables[0].Columns[2], DbType.DateTime).HavingCurrentUtcDateTimeAsDefault();

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Value(0, 0)));
        }

        private static bool AssertCurrentDateTimeValue(DateTime currentDateTime, DateTimeKind expecteDateTimeKind)
        {
            Assert.AreEqual(expecteDateTimeKind, currentDateTime.Kind);
            return Math.Abs((DateTime.Now - currentDateTime).TotalHours) <= 24;
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig6", "Id", "CurrentDateTime", "CurrentUtcDateTime")
                    {
                        {
                            1,
                            new Func<object, bool>(v => AssertCurrentDateTimeValue((DateTime)v, DateTimeKind.Unspecified)),
                            new Func<object, bool>(v => AssertCurrentDateTimeValue((DateTime)v, DateTimeKind.Unspecified)) // note: ADO.NET is always loading DATETIMEs as Kind=Unspecified since no additional information of the kind is retained
                        },
                    }
                };
            }
        }
    }
}