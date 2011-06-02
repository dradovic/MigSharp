using System.Collections.Generic;
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
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String)
                .WithNotNullableColumn(Tables[0].Columns[2], DbType.AnsiString);

            db.Execute(context =>
                {
                    bool isCe4Provider = db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4;
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;
                    IDataParameter text = command.AddParameter("@text", DbType.String, Tables[0].Value(0, 1));
                    if (isCe4Provider)
                    {
                        SetSqlDbTypeToNText(text);
                    }
                    IDataParameter ansiText = command.AddParameter("@ansiText", isCe4Provider ? DbType.String : DbType.AnsiString, Tables[0].Value(0, 2));
                    if (isCe4Provider)
                    {
                        SetSqlDbTypeToNText(ansiText);
                    }
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, {4})",
                        Tables[0].Name,
                        Tables[0].Columns[1],
                        Tables[0].Columns[2],
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

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig12", "Id", "Text", "AnsiText")
                    {
                        { 1, new string('s', 10000), new string('a', 10000) },
                    }
                };
            }
        }
    }
}