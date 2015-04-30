using System;
using System.Data;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test RowVersionColumn.")]
    internal class Migration21 : IIntegrationTestMigration
    {
        private static bool _rowVersionColumnIsSupported;

        public void Up(IDatabase db)
        {
            _rowVersionColumnIsSupported = db.Context.ProviderMetadata.Name.StartsWith("SqlServer", StringComparison.Ordinal);

            if (_rowVersionColumnIsSupported)
            {
                db.CreateTable("Mig21")
                  .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
                  .WithRowVersionColumn("Version")
                  .WithNotNullableColumn("Content", DbType.String).OfSize(255);
            }
            else
            {
                db.CreateTable("Mig21")
                  .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
                  .WithNullableColumn("Version", DbType.Int64)
                  .WithNotNullableColumn("Content", DbType.String).OfSize(255);
            }

            db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;

                    if (_rowVersionColumnIsSupported)
                    {
                        command.CommandText = "INSERT INTO \"Mig21\" ( \"Content\" ) VALUES ( 'First' )";
                        context.CommandExecutor.ExecuteNonQuery(command);

                        byte[] firstRowVersion;
                        if (IntegrationTestContext.IsScripting)
                        {
                            firstRowVersion = BitConverter.GetBytes(1L);
                        }
                        else
                        {
                            command.CommandText = "SELECT Version FROM \"Mig21\"";
                            firstRowVersion = (byte[])command.ExecuteScalar();
                        }

                        command.CommandText = "UPDATE \"Mig21\" SET Content = 'Updated'";
                        context.CommandExecutor.ExecuteNonQuery(command);

                        byte[] updatedRowVersion;
                        if (IntegrationTestContext.IsScripting)
                        {
                            updatedRowVersion = BitConverter.GetBytes(2L);
                        }
                        else
                        {
                            command.CommandText = "SELECT Version FROM \"Mig21\"";
                            updatedRowVersion = (byte[])command.ExecuteScalar();
                        }
                        CollectionAssert.AreNotEqual(firstRowVersion, updatedRowVersion, "The row version was not updated.");
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO \"Mig21\" ( \"Content\", \"Version\" ) VALUES ( 'Updated', 1 )";
                        context.CommandExecutor.ExecuteNonQuery(command);
                    }
                });
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig21", "Id", "Version", "Content")
                            {
                                {1, new Func<object, bool>(v => !_rowVersionColumnIsSupported || BitConverter.ToUInt64((byte[])v, 0) > 0), "Updated"}
                            }
                    };
            }
        }
    }
}