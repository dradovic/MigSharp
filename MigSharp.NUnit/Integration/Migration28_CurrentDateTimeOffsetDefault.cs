using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test HavingCurrentDateTimeOffsetAsDefault")]
    internal class Migration28 : IVersionConstrainedExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.DateTimeOffset).HavingCurrentDateTimeOffsetAsDefault();

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Value(0, 0)));
        }

        private static bool AssertCurrentDateTimeValue(DateTimeOffset currentDateTime)
        {
            return Math.Abs((DateTime.UtcNow - currentDateTime.UtcDateTime).TotalHours) <= 24;
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig28", "Id", "CurrentDateTimeOffset")
                    {
                        {
                            1,
                            new Func<object, bool>(v => AssertCurrentDateTimeValue((DateTimeOffset)v))
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
                    Platform.SQLite, 
                };
            }
        }

        public DbPlatform MinimumVersionSupportingFeatureUnderTest(Platform platform)
        {
            if (platform != Platform.SqlServer) throw new ArgumentOutOfRangeException("platform");

            return DbPlatform.SqlServer2012;
        }
    }
}