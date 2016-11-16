using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                    Assert.AreEqual("Schema27", context.StepMetadata.ModuleName);
                    Assert.AreEqual(27L, context.StepMetadata.Migrations.Single().Timestamp);
                    Assert.AreEqual("Test UseModuleNameAsDefaultSchema", context.StepMetadata.Migrations.Single().Tag);
                    Assert.AreEqual(MigrationDirection.Up, context.StepMetadata.Direction);
                    Assert.IsTrue(context.StepMetadata.UseModuleNameAsDefaultSchema, "Passing UseModuleNameAsDefaultSchema to migration metadata failed.");
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