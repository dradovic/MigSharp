using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using MigSharp.Core;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test adding/removing DF contraint for a table in a different schema")]
    internal class Migration29 : IVersionConstrainedExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            const string schemaName = "Schema 29";
            db.CreateSchema(schemaName);

            db.Schemata[schemaName].CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn("Id", DbType.Int32)
                .WithNotNullableColumn("Column With Default", DbType.DateTime2).HavingCurrentDateTimeAsDefault();

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"".""{1}"" (""{2}"") VALUES ('{3}')", schemaName, Tables[0].Name, Tables[0].Columns[0], 1));

            // the following ALTER statement has to DROP the old default constraint and create the new one
            db.Schemata[schemaName].Tables[Tables[0].Name].Columns["Column With Default"].AlterToNotNullable(DbType.DateTimeOffset).HavingCurrentDateTimeOffsetAsDefault();

            // renaming the column also needs to rename the default constraint using the schema prefix
            db.Schemata[schemaName].Tables[Tables[0].Name].Columns["Column With Default"].Rename("Column With Default II");

            // check if the default constraint still works
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"".""{1}"" (""{2}"") VALUES ('{3}')", schemaName, Tables[0].Name, Tables[0].Columns[0], 2));
        }

        private static bool AssertCurrentDateTimeValue(DateTimeOffset currentDateTime)
        {
            return Math.Abs((DateTimeOffset.UtcNow - currentDateTime).TotalHours) <= 24;
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable(new TableName("Mig 29", "Schema 29"), "Id", "Column With Default II")
                    {
                        {
                            1, new Func<object, bool>(v => AssertCurrentDateTimeValue((DateTimeOffset)v))
                        },
                        {
                            2, new Func<object, bool>(v => AssertCurrentDateTimeValue((DateTimeOffset)v))
                        },
                    }
                };
            }
        }

        public IEnumerable<Platform> PlatformsNotSupportingFeatureUnderTest
        {
            get
            {
                return new[]
                {
                        Platform.MySql,
                        Platform.Oracle,
                        Platform.Teradata,
                        Platform.SQLite
                };
            }
        }

        public DbPlatform MinimumVersionSupportingFeatureUnderTest(Platform platform)
        {
            if (platform != Platform.SqlServer) throw new ArgumentOutOfRangeException("platform");

            return DbPlatform.SqlServer2008;
        }
    }
}