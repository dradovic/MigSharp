using System.Data;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Core;
using MigSharp.Process;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Db.Type String with Maximum Size")]
    internal class Migration12 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.String)
                .WithNotNullableColumn(ColumnNames[2], DbType.AnsiString);

            db.Execute(context =>
                {
                    bool isCe4Provider = db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4;
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;
                    IDataParameter text = command.AddParameter("@text", DbType.String, ExpectedValues[0, 1]);
                    if (isCe4Provider)
                    {
                        SetSqlDbTypeToNText(text);
                    }
                    IDataParameter ansiText = command.AddParameter("@ansiText", isCe4Provider ? DbType.String : DbType.AnsiString, ExpectedValues[0, 2]);
                    if (isCe4Provider)
                    {
                        SetSqlDbTypeToNText(ansiText);
                    }
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, {4})",
                        TableName,
                        ColumnNames[1],
                        ColumnNames[2],
                        context.ProviderMetadata.GetParameterSpecifier(text),
                        context.ProviderMetadata.GetParameterSpecifier(ansiText));
                    Log.Verbose(LogCategory.Sql, command.CommandText);
                    int affectedRows = command.ExecuteNonQuery();
                    Assert.AreEqual(1, affectedRows);
                });
        }

        private static void SetSqlDbTypeToNText(IDataParameter parameter)
        {
            Debug.Assert(parameter.GetType().Name == "SqlCeParameter");
            parameter.GetType().GetProperty("SqlDbType").SetValue(parameter, 11, null); // 11: corresponds to SqlDbType.NText
        }

        public string TableName { get { return "Mig12"; } }
        public string[] ColumnNames { get { return new[] { "Id", "Text", "AnsiText" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, new string('s', 10000), new string('a', 10000) },
                };
            }
        }
    }
}