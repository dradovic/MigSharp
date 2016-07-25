using System.Collections.Generic;
using System.Data;
using MigSharp.Core;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test UseModuleNameAsDefaultSchema", UseModuleNameAsDefaultSchema = true, ModuleName = "Schema27")]
    internal class Migration27 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            const string schemaName = "Schema27";

            db.CreateSchema(schemaName);

            db.CreateTable("Mig27") // should be created in Schema27
              .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
              .WithNotNullableColumn("Content", DbType.String);
            db.Execute(@"INSERT INTO ""Schema27"".""Mig27"" ( ""Content"" ) VALUES ( 'Success' )");

            db.Execute(context =>
                {
                    Assert.AreEqual("Schema27", context.MigrationMetadata.ModuleName);
                    Assert.AreEqual(27L, context.MigrationMetadata.Timestamp);
                    Assert.AreEqual("Test UseModuleNameAsDefaultSchema", context.MigrationMetadata.Tag);
                    Assert.AreEqual(MigrationDirection.Up, context.MigrationMetadata.Direction);
                    Assert.IsTrue(context.MigrationMetadata.UseModuleNameAsDefaultSchema, "Passing UseModuleNameAsDefaultSchema to migration metadata failed.");
                });
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable(new TableName("Mig27", "Schema27"), "Id", "Content")
                            {
                                { 1, "Success" },
                            },
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
    }
}