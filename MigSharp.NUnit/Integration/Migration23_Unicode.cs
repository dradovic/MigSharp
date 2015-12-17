using System.Collections.Generic;
using System.Data;
using System.Globalization;
using MigSharp.Process;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Unicode Data")]
    internal class Migration23 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.CreateTable("Mig23a")
              .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
              .WithNotNullableColumn("Data", DbType.String).OfSize(255);
            db.CreateTable("Mig23b")
              .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
              .WithNotNullableColumn("Data", DbType.String).OfSize(255);

            // INSERT using literal SQL (something which could be considered bad practice is most cases)
            string unicodeLiteralPrefix = db.Context.ProviderMetadata.PrefixUnicodeLiterals ? "N" : string.Empty; // see: http://stackoverflow.com/questions/31270356/is-nsome-string-here-ansi-sql
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""Mig23a"" (""Data"") VALUES ({0}'Irgendöppis')", unicodeLiteralPrefix));
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""Mig23a"" (""Data"") VALUES ({0}'Unicodović')", unicodeLiteralPrefix));

            // INSERT using parameter injection
            db.Execute(ctx =>
                {
                    InsertUsingParameters(ctx, "Irgendöppis");
                    InsertUsingParameters(ctx, "Unicodović");
                });
        }

        private static void InsertUsingParameters(IRuntimeContext ctx, string value)
        {
            IDbCommand command = ctx.CreateCommand();
            IDataParameter parameter = command.AddParameter("@value", DbType.String, value);
            command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""Mig23b"" (""Data"") VALUES ({0})", ctx.ProviderMetadata.GetParameterSpecifier(parameter));
            ctx.CommandExecutor.ExecuteNonQuery(command);
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig23a", "Id", "Data")
                            {
                                { 1, "Irgendöppis" },
                                { 2, "Unicodović" }, // LATIN-1
                            },
                        new ExpectedTable("Mig23b", "Id", "Data")
                            {
                                { 1, "Irgendöppis" },
                                { 2, "Unicodović" }, // LATIN-1
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
                        Platform.Oracle, // although Oracle ODBC would support this test
                        Platform.Teradata, 
                    };
            }
        }
    }
}