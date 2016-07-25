using System.Data;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Process;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Db.Type String with Maximum Size")]
    internal class Migration12 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String)
                .WithNotNullableColumn(Tables[0].Columns[2], DbType.AnsiString);

            db.Execute(context =>
                {
                    IDbCommand command = context.CreateCommand();
                    IDataParameter text = command.AddParameter("@text", DbType.String, Tables[0].Value(0, 1));
                    IDataParameter ansiText = command.AddParameter("@ansiText", DbType.AnsiString, Tables[0].Value(0, 2));
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, {4})",
                        Tables[0].Name,
                        Tables[0].Columns[1],
                        Tables[0].Columns[2],
                        context.ProviderMetadata.GetParameterSpecifier(text),
                        context.ProviderMetadata.GetParameterSpecifier(ansiText));
                    context.CommandExecutor.ExecuteNonQuery(command);
                });
        }

        public ExpectedTables Tables
        {
            get
            {
                int numberOfChars = IntegrationTestContext.IsScripting ? 1000 : 10000; // Oracle does not like too long text in SQL scripts (ORA-01704: string literal too long)
                return new ExpectedTables
                {
                    new ExpectedTable("Mig12", "Id", "Text", "AnsiText")
                    {
                        { 1, new string('s', numberOfChars), new string('a', numberOfChars) },
                    }
                };
            }
        }
    }
}