using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

using MigSharp.Process;
using MigSharp.Providers;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test Creating a Table Containing All Supported Data Types")]
    internal class Migration8 : IIntegrationTestMigration
    {
        // see Mapping CLR Parameter Data: http://msdn.microsoft.com/en-us/library/ms131092.aspx
        private static readonly Dictionary<DbType, object> TestValues = new Dictionary<DbType, object>
        {
            { DbType.AnsiString, "Test" },
            { DbType.Binary, new byte[] { 123, byte.MinValue, byte.MaxValue } },
            { DbType.Byte, Byte.MaxValue },
            { DbType.Boolean, true },
            { DbType.Date, new DateTime(2010, 12, 28, 18, 14, 33).Date },
            { DbType.DateTime, new DateTime(2010, 12, 28, 18, 14, 33) },
            { DbType.Decimal, 0.12345 },
            { DbType.Double, 3.14159265358979d },
            { DbType.Guid, new Guid("40c3290e-8ad9-4b27-add5-2602edb72d0e") },
            { DbType.Int16, Int16.MaxValue },
            { DbType.Int32, Int32.MaxValue },
            { DbType.Int64, Int64.MaxValue },
            { DbType.SByte, SByte.MinValue },
            { DbType.Single, 2.71828182845904f },
            { DbType.String, "Irgendöppis" }, // FIXME: don, "Unicodović" should work as well (see Migration5) 
            { DbType.Time, DateTime.Parse("12/28/2010 19:25:21.9999", CultureInfo.InvariantCulture).TimeOfDay },
            { DbType.UInt16, UInt16.MinValue },
            { DbType.UInt32, UInt32.MinValue },
            { DbType.UInt64, UInt64.MinValue },
            { DbType.VarNumeric, 1.5f },
            { DbType.DateTimeOffset, new DateTimeOffset(2010, 12, 28, 18, 14, 33, TimeSpan.FromHours(-2.0)) },
            { DbType.DateTime2, DateTime.Parse("12/28/2010 19:25:21.9999", CultureInfo.InvariantCulture) },
        };

        private static readonly ExpectedTables ExpectedTables = new ExpectedTables();

        public void Up(IDatabase db)
        {
            const string tableName = "Mig8";
            ExpectedTables.Clear();

            // create a table that contains columns for all supported data types
            ICreatedTable table = db.CreateTable(tableName);
            Dictionary<string, DbType> columns = new Dictionary<string, DbType>();
            int i = 1;
            foreach (SupportsAttribute support in IntegrationTestContext.SupportsAttributes
                .Where(s => !IntegrationTestContext.IsScripting || s.IsScriptable)
                .OrderByDescending(s => s.MaximumSize)) // make sure the first column is not a LOB column as Teradata automatically adds an index to the first column and then would crash with: 'Cannot create index on LOB columns.'
            {
                if (support.DbType == DbType.AnsiStringFixedLength || // skip fixed length character types as the table would grow too large
                    support.DbType == DbType.StringFixedLength || // skip fixed length character types as the table would grow too large
                    support.DbType == DbType.Int64 || // skip Int64 as the ODBC driver does not support DbParameters for this data type --> note that Int64 is implicitly tested as MigSharp uses this data type for its timestamp column
                    (support.DbType == DbType.Decimal && support.MaximumScale == 0)) // this is test thoroughly in Migration11
                {
                    continue;
                }

                string columnName = "Column" + i++;
                table.WithNullableColumn(columnName, support.DbType).OfSize(support.MaximumSize, support.MaximumScale);
                columns.Add(columnName, support.DbType);
            }

            db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;

                    ExpectedTables[0].Clear();
                    var values = new List<object>();
                    foreach (var column in columns)
                    {
                        DbType dbType;
                        object value = GetTestValue(column, db, out dbType);
                        command.AddParameter("@" + column.Key, (dbType == DbType.AnsiString && (db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe35 || db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4)) ? DbType.String : dbType, value);
                        values.Add(value);
                    }
                    ExpectedTables[0].Add(values.ToArray());

                    command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" ({1}) VALUES ({2})",
                        Tables[0].Name,
                        string.Join(", ", columns.Keys.Select(c => "\"" + c + "\"").ToArray()),
                        string.Join(", ", command.Parameters.Cast<IDbDataParameter>().Select(p => context.ProviderMetadata.GetParameterSpecifier(p)).ToArray()));
                    context.CommandExecutor.ExecuteNonQuery(command);
                });

            ExpectedTables.Add(new ExpectedTable(tableName, columns.Keys));

            // create a table for each supported primary key data type
            // (as combining them all into one table would be too much)
            foreach (SupportsAttribute support in IntegrationTestContext.SupportsAttributes
                .Where(s => (!IntegrationTestContext.IsScripting || s.IsScriptable) && s.CanBeUsedAsPrimaryKey))
            {
                string pkTableName = Tables[0].Name + "WithPkOf" + support.DbType + support.MaximumScale;
                ICreatedTable pkTable = db.CreateTable(pkTableName);
                int maximumSize = support.MaximumSize;
                if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2005 ||
                    db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2005Odbc ||
                    db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2008 ||
                    db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2012)
                {
                    // SQL Server only allow PKs with a maximum length of 900 bytes
                    if (support.DbType == DbType.AnsiStringFixedLength)
                    {
                        maximumSize = 900; // FEATURE: this information should be part of the SupportAttribute
                    }
                    if (support.DbType == DbType.StringFixedLength)
                    {
                        maximumSize = 450; // FEATURE: this information should be part of the SupportAttribute
                    }
                }
                pkTable.WithPrimaryKeyColumn("Id", support.DbType).OfSize(maximumSize, support.MaximumScale);
                ExpectedTables.Add(new ExpectedTable(pkTableName, "Id"));
            }
        }

        private static object GetTestValue(KeyValuePair<string, DbType> column, IDatabase db, out DbType type)
        {
            type = column.Value;
            object result = TestValues[column.Value];

            // Special treatment of certain data types for certain providers.
            // IMPORTANT: All these DbTypes should be marked with a Warning in the SupportsAttribute declaration!
            if (db.Context.ProviderMetadata.Name.Contains("Oracle") ||
                db.Context.ProviderMetadata.Name.Contains("Teradata"))
            {
                if (result is Guid)
                {
                    type = DbType.Binary;
                    return ((Guid)result).ToByteArray();
                }
                if (result is byte)
                {
                    type = DbType.Int32;
                    return Convert.ToInt32(result, CultureInfo.InvariantCulture);
                }
            }
            if (db.Context.ProviderMetadata.Name.Contains("Oracle") ||
                db.Context.ProviderMetadata.Name.Contains("Teradata") ||
                db.Context.ProviderMetadata.Name == ProviderNames.SQLite)
            {
                if (result is bool)
                {
                    type = DbType.Int32;
                    return Convert.ToInt32((bool)result, CultureInfo.InvariantCulture);
                }
            }
            return result;
        }

        public ExpectedTables Tables { get { return ExpectedTables; } }
    }
}