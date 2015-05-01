using System;
using System.Data;
using System.Globalization;
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

            db.CreateTable("Mig21b")
                  .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
                  .WithNotNullableColumn("Content", DbType.String).OfSize(255);

            if (_rowVersionColumnIsSupported)
            {
                db.CreateTable("Mig21a")
                  .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
                  .WithRowVersionColumn("Version")
                  .WithNotNullableColumn("Content", DbType.String).OfSize(255);
                db.Tables["Mig21b"].AddRowVersionColumn("Version");
            }
            else
            {
                db.CreateTable("Mig21a")
                  .WithPrimaryKeyColumn("Id", DbType.Int32).AsIdentity()
                  .WithNotNullableColumn("Version", DbType.Int64)
                  .WithNotNullableColumn("Content", DbType.String).OfSize(255);
                db.Tables["Mig21b"].AddNotNullableColumn("Version", DbType.Int64);
            }

            db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;

                    InsertAndUpdateRow(command, "Mig21a", context);
                    InsertAndUpdateRow(command, "Mig21b", context);
                });
        }

        private static void InsertAndUpdateRow(IDbCommand command, string tableName, IRuntimeContext context)
        {
            if (_rowVersionColumnIsSupported)
            {
                command.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" ( \"Content\" ) VALUES ( 'First' )", tableName);
                context.CommandExecutor.ExecuteNonQuery(command);

                byte[] firstRowVersion;
                if (IntegrationTestContext.IsScripting)
                {
                    firstRowVersion = BitConverter.GetBytes(1L);
                }
                else
                {
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT Version FROM \"{0}\"", tableName);
                    firstRowVersion = (byte[])command.ExecuteScalar();
                }

                command.CommandText = string.Format(CultureInfo.InvariantCulture, "UPDATE \"{0}\" SET Content = 'Updated'", tableName);
                context.CommandExecutor.ExecuteNonQuery(command);

                byte[] updatedRowVersion;
                if (IntegrationTestContext.IsScripting)
                {
                    updatedRowVersion = BitConverter.GetBytes(2L);
                }
                else
                {
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT Version FROM \"{0}\"", tableName);
                    updatedRowVersion = (byte[])command.ExecuteScalar();
                }
                CollectionAssert.AreNotEqual(firstRowVersion, updatedRowVersion, "The row version was not updated.");
            }
            else
            {
                command.CommandText = string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" ( \"Content\", \"Version\" ) VALUES ( 'Updated', 1 )", tableName);
                context.CommandExecutor.ExecuteNonQuery(command);
            }
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig21a", "Id", "Version", "Content")
                            {
                                {1, CheckRowVersionValue(), "Updated"}
                            },
                        new ExpectedTable("Mig21b", "Id", "Content", "Version")
                            {
                                {1, "Updated", CheckRowVersionValue()}
                            }
                    };
            }
        }

        private static Func<object, bool> CheckRowVersionValue()
        {
            return v => !_rowVersionColumnIsSupported || BitConverter.ToUInt64((byte[])v, 0) > 0;
        }
    }
}