using System;
using System.Data;
using System.Globalization;
using System.Linq;
using MigSharp.Process;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "DateTime2 with different precisions.")]
    internal class Migration19 : IIntegrationTestMigration
    {
        private static readonly DateTime TestValue = new DateTime(2014, 5, 17, 16, 55, 34, 123);

        public void Up(IDatabase db)
        {
            //DateTime.Parse("12/28/2010 19:25:21.9999", CultureInfo.InvariantCulture)

            db.CreateTable(Tables[0].Name)
              .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
              .WithNotNullableColumn(Tables[0].Columns[1], DbType.DateTime2)
              .WithNotNullableColumn(Tables[0].Columns[2], DbType.DateTime2).OfSize(7)
              .WithNotNullableColumn(Tables[0].Columns[3], DbType.DateTime2).OfSize(3)
              .WithNotNullableColumn(Tables[0].Columns[4], DbType.DateTime2).OfSize(1)
              .WithNotNullableColumn(Tables[0].Columns[5], DbType.DateTime2).OfSize(0);

            db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;
                    command.AddParameter("@" + Tables[0].Columns[1], DbType.DateTime2, TestValue);
                    command.AddParameter("@" + Tables[0].Columns[2], DbType.DateTime2, TestValue);
                    command.AddParameter("@" + Tables[0].Columns[3], DbType.DateTime2, TestValue);
                    command.AddParameter("@" + Tables[0].Columns[4], DbType.DateTime2, TestValue);
                    command.AddParameter("@" + Tables[0].Columns[5], DbType.DateTime2, TestValue);
                    command.CommandText = string.Format(CultureInfo.InvariantCulture,
                        "INSERT INTO \"{0}\" (\"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\") VALUES ('{7}', {8})",
                        Tables[0].Name,
                        Tables[0].Columns[0],
                        Tables[0].Columns[1],
                        Tables[0].Columns[2],
                        Tables[0].Columns[3],
                        Tables[0].Columns[4],
                        Tables[0].Columns[5],
                        Tables[0].Value(0, 0),
                        string.Join(", ",
                            command.Parameters.Cast<IDbDataParameter>()
                                   .Select(p => context.ProviderMetadata.GetParameterSpecifier(p))
                                   .ToArray()));
                    context.CommandExecutor.ExecuteNonQuery(command);
                });
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig19", "Id", "DateWithoutSizeOf", "Date7", "Date3", "Date1", "Date0")
                            {
                                {
                                    1,
                                    TestValue, // should be the full value
                                    TestValue, // should be the full value
                                    TestValue, // should be the full value (still 3)
                                    new DateTime(TestValue.Year, TestValue.Month, TestValue.Day, TestValue.Hour, TestValue.Minute, TestValue.Second, 100*(TestValue.Millisecond/100)), // the seconds should only have one decimal place
                                    new DateTime(TestValue.Year, TestValue.Month, TestValue.Day, TestValue.Hour, TestValue.Minute, TestValue.Second, 0) // there should be no milliseconds
                                },
                            }
                    };
            }
        }
    }
}