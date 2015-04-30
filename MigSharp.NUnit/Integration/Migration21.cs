using System;
using System.Data;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test RowVersionColumn.")]
    internal class Migration21 : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            bool rowVersionColumnIsSupported = true; // db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe35 &&
            //db.Context.ProviderMetadata.Name != ProviderNames.SqlServerCe4 &&
            //db.Context.ProviderMetadata.Name != ProviderNames.Teradata &&
            //db.Context.ProviderMetadata.Name != ProviderNames.TeradataOdbc;
            if (!rowVersionColumnIsSupported)
            {
                return;
            }

            db.CreateTable("Mig21")
              .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
              .WithRowVersionColumn("Version")
              .WithNotNullableColumn("Content", DbType.String).OfSize(255);

            db.Execute(context =>
                {
                    //if (IntegrationTestContext.IsScripting) return;

                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;

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
                                {1, new Func<object, bool>(v => BitConverter.ToUInt64((byte[])v, 0) > 0), "Updated"}
                            }
                    };
            }
        }
    }
}