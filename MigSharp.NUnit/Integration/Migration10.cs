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
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(100);
            db.Execute(GetInsertStatement(0));
            db.Execute(GetInsertStatement(1));

            if (db.Context.ProviderMetadata.Name != ProviderNames.SQLite)
            {
                // add two new columns one of int and one of string
                db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[2], DbType.String).OfSize(100).HavingTemporaryDefault(DefaultString);
                db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[3], DbType.Int32).HavingTemporaryDefault(DefaultInt);
                db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[4], DbType.DateTime).HavingTemporaryDefault(DefaultDate);

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
                        catch (Exception x)
                        {
                            if (!x.IsDbException())
                            {
                                throw;
                            }
                            // a DbException is expected (for the case of a SqlServer35 the SqlCeException is not derived from DbException)
                            if (!(x is DbException) && x.GetType().Name != "SqlCeException")
                            {
                                throw;
                            }
                        }
                    });
            }
            else
            {
                // SQLite does not support dropping of default values
                db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[2], DbType.String).OfSize(100).HavingDefault(DefaultString);
                db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[3], DbType.Int32).HavingDefault(DefaultInt);
                db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[4], DbType.DateTime).HavingDefault(DefaultDate);
            }
        }

        private string GetInsertStatement(int row)
        {
            return string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[1], Tables[0].Value(row, 1));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig10", "Id", "CreatedCol", "AddedStrCol", "AddedIntCol", "AddedDateTimeCol")
                    {
                        { 1, "strval1", DefaultString, DefaultInt, DefaultDate },
                        { 2, "strval2", DefaultString, DefaultInt, DefaultDate },
                    }
                };
            }
        }
    }
}