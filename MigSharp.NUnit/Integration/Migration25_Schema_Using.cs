using System.Collections.Generic;
using System.Data;
using MigSharp.Core;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test database schemas")]
    internal class Migration25 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.CreateSchema("Schema25");

            // create and populate parent table
            db.Schemata["Schema25"].CreateTable("Mig25")
                                   .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
                                   .WithNotNullableColumn("Content", DbType.String);
            db.Execute(@"INSERT INTO ""Schema25"".""Mig25"" ( ""Content"" ) VALUES ( 'Success' )");

            // create and populate child table
            db.Schemata["Schema25"].CreateTable("Mig25 Child")
                                   .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
                                   .WithNotNullableColumn("ParentId", DbType.Int32);
            db.Execute(@"INSERT INTO ""Schema25"".""Mig25 Child"" ( ""ParentId"" ) VALUES ( 1 )");

            // test creating a FK within the same schema
            db.Schemata["Schema25"].Tables["Mig25 Child"].AddForeignKeyTo("Mig25")
                                                         .Through("ParentId", "Id");

            // test creating a FK to a table in another schema
            db.CreateTable("Mig25inDbo")
              .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
              .WithNotNullableColumn("Content", DbType.String);
            db.Execute(@"INSERT INTO ""dbo"".""Mig25inDbo"" ( ""Content"" ) VALUES ( 'Success' )");
            db.Schemata["Schema25"].Tables["Mig25 Child"].AddForeignKeyTo("Mig25inDbo")
                                                          .InSchema("dbo")
                                                          .Through("ParentId", "Id");

            // test renaming within a schema
            db.Schemata["Schema25"].Tables["Mig25"].Rename("Mig25 Renamed");
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable(new TableName("Mig25 Renamed", "Schema25"), "Id", "Content")
                            {
                                { 1, "Success" },
                            },
                        new ExpectedTable(new TableName("Mig25 Child", "Schema25"), "Id", "ParentId")
                            {
                                { 1, 1 }
                            },
                        new ExpectedTable(new TableName("Mig25inDbo", "dbo"), "Id", "Content")
                            {
                                { 1, "Success" }
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
                        Platform.SQLite
                    };
            }
        }
    }
}