using System.Data;
using System.Data.Common;
using System.Globalization;

using MigSharp.Core;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test decimals")]
    internal class Migration11 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.Decimal).OfSize(3)
                .WithNotNullableColumn(ColumnNames[2], DbType.Decimal).OfSize(5, 2);

            db.Execute(GetInsertStatement((decimal)ExpectedValues[0, 2] + 0.003m));
            db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;
                    command.CommandText = GetInsertStatement(1000m);
                    Log.Verbose(LogCategory.Sql, command.CommandText);
                    try
                    {
                        command.ExecuteNonQuery();
                        Assert.Fail("The previous query should have failed.");
                    }
                    catch (DbException)
                    {
                        // this is expected
                    }
                });
        }

        private string GetInsertStatement(decimal value2)
        {
            return string.Format(CultureInfo.InvariantCulture,
                @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, {4})", TableName, ColumnNames[1], ColumnNames[2], ExpectedValues[0, 1], value2);
        }

        public string TableName { get { return "Mig11"; } }
        public string[] ColumnNames { get { return new[] { "Id", "Dec3", "Dec3_2" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, 333m, 333.33m },
                };
            }
        }
    }
}