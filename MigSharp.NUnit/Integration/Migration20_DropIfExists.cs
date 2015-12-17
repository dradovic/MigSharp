using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test DropIfExists.")]
    internal class Migration20 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            AssertMig20DoesNotExist(db);

            db.Tables["Mig20"].DropIfExists();

            db.CreateTable("Mig20")
              .WithPrimaryKeyColumn("Id", DbType.Int32);

            AssertMig20Exists(db);

            db.Tables["Mig20"].DropIfExists();

            AssertMig20DoesNotExist(db);
        }

        private static void AssertMig20Exists(IDatabase db)
        {
            AssertMig20(db, true);
        }

        private static void AssertMig20DoesNotExist(IDatabase db)
        {
            AssertMig20(db, false);
        }

        private static void AssertMig20(IDatabase db, bool exists)
        {
            if (IntegrationTestContext.IsScripting) return;

            db.Execute(context =>
                {
                    IDbCommand command = context.CreateCommand();
                    command.CommandText = @"SELECT * FROM ""Mig20""";
                    try
                    {
                        context.CommandExecutor.ExecuteNonQuery(command);
                        if (!exists)
                        {
                            Assert.Fail("The table 'Mig20' should not exist at this point.");
                        }
                    }
                    catch (Exception x)
                    {
                        if (!x.IsDbException())
                        {
                            throw;
                        }
                        if (exists)
                        {
                            Assert.Fail("The table 'Mig20' should exist at this point. But there an error occurred: {0}", x.Message);
                        }
                    }
                });
        }

        public ExpectedTables Tables { get { return new ExpectedTables(); } }

        public IEnumerable<Platform> PlatformsNotSupportingFeatureUnderTest
        {
            get
            {
                return new[]
                    {
                        Platform.SqlServerCe,
                        Platform.Teradata,
                    };
            }
        }
    }
}