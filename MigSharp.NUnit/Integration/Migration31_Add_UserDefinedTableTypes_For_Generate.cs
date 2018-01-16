using System.Collections.Generic;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport]
    internal class Migration31 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.Execute("CREATE TYPE [dbo].[Mig31 TableType] AS TABLE ( Id INT NOT NULL )");
        }

        public ExpectedTables Tables { get { return new ExpectedTables(); } }

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