using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using MigSharp.Core;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test decimals")]
    internal class Migration11 : IExclusiveIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }
            MySqlHelper.ActivateStrictMode(db);

            db.CreateTable(Tables[0].Name)
              .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32).AsIdentity()
              .WithNotNullableColumn(Tables[0].Columns[1], DbType.Decimal).OfSize(3)
              .WithNotNullableColumn(Tables[0].Columns[2], DbType.Decimal).OfSize(5, 2);

            db.Execute(GetInsertStatement((decimal)Tables[0].Value(0, 2) + 0.003m)); // the extra precision should be cut off silently
            db.Execute(context =>
                {
                    IDbCommand command = context.CreateCommand();
                    command.CommandText = GetInsertStatement(1000m);
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
                    }
                });
        }

        private string GetInsertStatement(decimal value2)
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 @"INSERT INTO ""{0}"" (""{1}"", ""{2}"") VALUES ({3}, {4})", Tables[0].Name, Tables[0].Columns[1], Tables[0].Columns[2], Tables[0].Value(0, 1), value2);
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig11", "Id", "Dec3", "Dec3_2")
                            {
                                { 1, 333m, 333.33m },
                            }
                    };
            }
        }

        public IEnumerable<Platform> PlatformsNotSupportingFeatureUnderTest { get { return new[] { Platform.SQLite }; } } // SQLite uses adaptiv algorithms for their data types: http://www.sqlite.org/datatype3.html
    }
}