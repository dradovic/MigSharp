using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using JetBrains.Annotations;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test database schemas")]
    internal class Migration26 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            const string schemaName = "Schema26";

            db.CreateSchema(schemaName);

            // make sure the schema was created
            AssertSchema(db, schemaName, true, "Creating Schema26 failed.");

            db.Schemata[schemaName].Drop();

            // make sure the schema does not exist anymore
            AssertSchema(db, schemaName, false, "Dropping Schema26 failed.");
        }

        [AssertionMethod]
        private static void AssertSchema(IDatabase db, string schemaName, bool exists, string failMessage)
        {
            if (IntegrationTestContext.IsScripting) return;

            db.Execute(context =>
                {
                    IDbCommand command = context.CreateCommand();
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [INFORMATION_SCHEMA].[SCHEMATA] WHERE [SCHEMA_NAME] = '{0}'", schemaName);
                    object count = command.ExecuteScalar();
                    if (!(Convert.ToInt32(count, CultureInfo.InvariantCulture) == 0 ^ exists))
                    {
                        Assert.Fail(failMessage);
                    }
                });
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables();
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
    }
}