using System.Data;
using System.Globalization;

using MigSharp.Core;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test the Execute feature")]
    internal class Migration3 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithNotNullableColumn(ColumnNames[0], DbType.String).OfSize(255);

            // we execute queries in 3 stages in order to make sure that the call-back is done at the correct moment

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" ( ""{1}"" ) VALUES ('Stage 1')",
                TableName,
                ColumnNames[0]));

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
                        TableName,
                        ColumnNames[0]);
                    Log.Verbose(LogCategory.Sql, command.CommandText);
                    int affectedRows = command.ExecuteNonQuery();
                    Assert.AreEqual(1, affectedRows, "Failed to insert a row.");
                });

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"UPDATE ""{0}"" SET ""{1}"" = '{2}' WHERE ""{1}"" = 'Stage 2'", 
                TableName, 
                ColumnNames[0], 
                ExpectedValues[0, 0]));
        }

        public string TableName { get { return "Mig3"; } }
        public string[] ColumnNames { get { return new[] { "Content" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { "Stage 3" },
                };
            }
        }
    }
}