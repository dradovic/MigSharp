using System.Data;
using System.Globalization;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test the Execute feature")]
    internal class Migration3 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithNotNullableColumn(Tables[0].Columns[0], DbType.String).OfSize(255);

            // we execute queries in 3 stages in order to make sure that the call-back is done at the correct moment

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" ( ""{1}"" ) VALUES ('Stage 1')",
                Tables[0].Name,
                Tables[0].Columns[0]));

            db.Execute(context =>
                {
                    Assert.IsNotNull(context);
                    Assert.IsNotNull(context.Connection);
                    Assert.AreEqual(ConnectionState.Open, context.Connection.State);
                    if (context.ProviderMetadata.SupportsTransactions)
                    {
                        Assert.IsNotNull(context.Transaction);
                    }
                    else
                    {
                        Assert.IsNull(context.Transaction);
                    }

                    // update row
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}"" = 'Stage 2' WHERE ""{1}"" = 'Stage 1'",
                        Tables[0].Name,
                        Tables[0].Columns[0]);
                    context.CommandExecutor.ExecuteNonQuery(command);
                });

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}"" = '{2}' WHERE ""{1}"" = 'Stage 2'",
                Tables[0].Name,
                Tables[0].Columns[0],
                Tables[0].Value(0, 0)));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig3", "Content")
                    {
                       "Stage 3",
                    }
                };
            }
        }
    }
}