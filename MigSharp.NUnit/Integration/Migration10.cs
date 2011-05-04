using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

using MigSharp.Core;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Temporary default values")]
    internal class Migration10 : IIntegrationTestMigration
    {
        private const string DefaultString = "StringDefault";
        private const int DefaultInt = 10;
        private static readonly DateTime DefaultDate = new DateTime(2011, 2, 1, 10, 58, 21); // 1st of Feb.

        public void Up(IDatabase db)
        {
            db.CreateTable(TableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(ColumnNames[1], DbType.String).OfSize(100);
            db.Execute(GetInsertStatement(0));
            db.Execute(GetInsertStatement(1));

            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                // add two new columns one of int and one of string
                db.Tables[TableName].AddNotNullableColumn(ColumnNames[2], DbType.String).OfSize(100).HavingTemporaryDefault(DefaultString);
                db.Tables[TableName].AddNotNullableColumn(ColumnNames[3], DbType.Int32).HavingTemporaryDefault(DefaultInt);
                db.Tables[TableName].AddNotNullableColumn(ColumnNames[4], DbType.DateTime).HavingTemporaryDefault(DefaultDate);

                // ensure that the default values have been droped
                db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;
                    command.CommandText = GetInsertStatement(1);
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
            else
            {
                // SQLite does not support dropping of default values
                db.Tables[TableName].AddNotNullableColumn(ColumnNames[2], DbType.String).OfSize(100).HavingDefault(DefaultString);
                db.Tables[TableName].AddNotNullableColumn(ColumnNames[3], DbType.Int32).HavingDefault(DefaultInt);
                db.Tables[TableName].AddNotNullableColumn(ColumnNames[4], DbType.DateTime).HavingDefault(DefaultDate);
            }
        }

        private string GetInsertStatement(int row)
        {
            return string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", TableName, ColumnNames[1], ExpectedValues[row, 1]);
        }

        public string TableName { get { return "Mig10"; } }
        public string[] ColumnNames { get { return new[] { "Id", "CreatedCol", "AddedStrCol", "AddedIntCol", "AddedDateTimeCol" }; } }
        public object[,] ExpectedValues
        {
            get
            {
                return new object[,]
                {
                    { 1, "strval1", DefaultString, DefaultInt, DefaultDate },
                    { 2, "strval2", DefaultString, DefaultInt, DefaultDate },
                };
            }
        }
    }
}