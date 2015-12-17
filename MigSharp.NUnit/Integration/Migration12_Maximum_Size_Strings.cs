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
                    bool isCeProvider = db.Context.ProviderMetadata.Platform == Platform.SqlServerCe;
                    IDbCommand command = context.CreateCommand();
                    IDataParameter text = command.AddParameter("@text", DbType.String, Tables[0].Value(0, 1));
                    if (isCeProvider)
                    {
                        SetSqlDbTypeToNText(text);
                    }
                    IDataParameter ansiText = command.AddParameter("@ansiText", isCeProvider ? DbType.String : DbType.AnsiString, Tables[0].Value(0, 2));
                    if (isCeProvider)
                    {
                        SetSqlDbTypeToNText(ansiText);
                    }
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, {4})",
                        Tables[0].Name,
                        Tables[0].Columns[1],
                        Tables[0].Columns[2],
                        context.ProviderMetadata.GetParameterSpecifier(text),
                        context.ProviderMetadata.GetParameterSpecifier(ansiText));
                    context.CommandExecutor.ExecuteNonQuery(command);
                });
        }

        private static void SetSqlDbTypeToNText(IDataParameter parameter)
        {
            Debug.Assert(parameter.GetType().Name == "SqlCeParameter");
            parameter.GetType().GetProperty("SqlDbType").SetValue(parameter, 11, null); // 11: corresponds to SqlDbType.NText
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