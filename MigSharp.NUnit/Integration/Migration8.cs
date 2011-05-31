using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

using MigSharp.Core;
using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

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
            { DbType.Single, Single.MaxValue },
            { DbType.String, "Irgendöppis" }, // FIXME: don, "Unicodović" should work as well (see Migration5) 
            { DbType.Time, DateTime.Parse("12/28/2010 19:25:21.9999", CultureInfo.InvariantCulture).TimeOfDay },
            { DbType.UInt16, UInt16.MinValue },
            { DbType.UInt32, UInt32.MinValue },
            { DbType.UInt64, UInt64.MinValue },
            { DbType.VarNumeric, 1.5f },
            { DbType.DateTimeOffset, new DateTimeOffset(2010, 12, 28, 18, 14, 33, TimeSpan.FromHours(-2.0)) },
            { DbType.DateTime2, DateTime.Parse("12/28/2010 19:25:21.9999", CultureInfo.InvariantCulture) },
        };

        private static readonly Dictionary<string, DbType> Columns = new Dictionary<string, DbType>();
        private static readonly List<object> Values = new List<object>();
        private static IEnumerable<SupportsAttribute> _supports;

        // CLEAN: this static information should be moved somewhere else (a integration test context class)
        // where it is accessible to all migrations.
        internal static void Initialize(IEnumerable<SupportsAttribute> supports)
        {
            _supports = supports;
        }

        public void Up(IDatabase db)
        {
            // create a table that contains columns for all supported data types
            ICreatedTable table = db.CreateTable(TableName);
            Columns.Clear();
            int i = 1;
            foreach (SupportsAttribute support in _supports
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
                ICreatedTableWithAddedColumn column = table.WithNullableColumn(columnName, support.DbType);
                if (support.MaximumSize >= 0)
                {
                    if (support.MaximumScale >= 0)
                    {
                        column.OfSize(support.MaximumSize, support.MaximumScale);
                    }
                    else
                    {
                        column.OfSize(support.MaximumSize);
                    }
                }

                Columns.Add(columnName, support.DbType);
            }

            db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;

                    Values.Clear();
                    foreach (var column in Columns)
                    {
                        DbType dbType;
                        object value = GetTestValue(column, db, out dbType);
                        command.AddParameter("@" + column.Key, (dbType == DbType.AnsiString && db.Context.ProviderMetadata.Name == ProviderNames.SqlServerCe4) ? DbType.String : dbType, value);
                        Values.Add(value);
                    }

                    command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" ({1}) VALUES ({2})",
                        TableName,
                        string.Join(", ", Columns.Keys.Select(c => "\"" + c + "\"").ToArray()),
                        string.Join(", ", command.Parameters.Cast<IDbDataParameter>().Select(p => context.ProviderMetadata.GetParameterSpecifier(p)).ToArray()));
                    Log.Verbose(LogCategory.Sql, command.CommandText);

                    //Trace.WriteLine("Migration8: executing: " + command.CommandText);
                    int affectedRows = command.ExecuteNonQuery();
                    Assert.AreEqual(1, affectedRows);
                });

            // create a table for each supported primary key data type
            // (as combining them all into one table would be too much)
            foreach (SupportsAttribute support in _supports
                .Where(s => s.CanBeUsedAsPrimaryKey))
            {
                ICreatedTable pkTable = db.CreateTable(TableName + "WithPkOf" + support.DbType + support.MaximumScale);
                int maximumSize = support.MaximumSize;
                if (db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2005 ||
                    db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2005Odbc ||
                    db.Context.ProviderMetadata.Name == ProviderNames.SqlServer2008)
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

        public string TableName { get { return "Mig8"; } }
        public string[] ColumnNames { get { return Columns.Keys.ToArray(); } }
        public object[,] ExpectedValues
        {
            get
            {
                var expectedValues = new object[1,Values.Count];
                int i = 0;
                foreach (object value in Values)
                {
                    expectedValues[0, i++] = value;
                }
                return expectedValues;
            }
        }
    }
}