using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.SQLite, InvariantName, MaximumDbObjectNameLength = 128)]
    [Supports(DbType.AnsiString, MaximumSize = int.MaxValue, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.AnsiString)]
    [Supports(DbType.Binary)]
    [Supports(DbType.Byte, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Boolean, CanBeUsedAsPrimaryKey = true, Warning = "Requires custom ADO.NET code to convert to/from an Int32 (using System.Convert).")]
    [Supports(DbType.Date, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.DateTime, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Decimal, MaximumSize = 28, MaximumScale = 28, CanBeUsedAsPrimaryKey = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Decimal, MaximumSize = 28, CanBeUsedAsPrimaryKey = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Double)]
    [Supports(DbType.Guid, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Int16, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Int32, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Int64, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.SByte, CanBeUsedAsPrimaryKey = true)]
    //[Supports(DbType.Single)] throws a System.OverflowException in the SQLiteDataReader when trying to convert the data type to a decimal!?
    [Supports(DbType.String, MaximumSize = 4000, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.String)]
    [Supports(DbType.UInt16, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.UInt32, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.UInt64, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.VarNumeric)]
    [Supports(DbType.AnsiStringFixedLength, MaximumSize = int.MaxValue, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.StringFixedLength, MaximumSize = int.MaxValue, CanBeUsedAsPrimaryKey = true)]
    internal class SQLiteProvider : IProvider
    {
        public const string InvariantName = "System.Data.SQLite";

        public string ExistsTable(string databaseName, string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM sqlite_master WHERE name='{0}'", tableName);
        }

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, @"CREATE TABLE ""{0}"" ({1}{2}{1})",
                tableName,
                Environment.NewLine,
                string.Join(", " + Environment.NewLine, columns.Select(GetColumnDefinition)
                    .Concat(GetTableConstraints(columns, primaryKeyConstraintName)).ToArray()));
            foreach (IGrouping<string, CreatedColumn> uniqueColumns in columns
                .Where(c => !string.IsNullOrEmpty(c.UniqueConstraint))
                .GroupBy(c => c.UniqueConstraint))
            {
                // FIXME: is okay to implement uniqueness using unique indexes insted of the UNIQUE key word at the column definition?
                yield return AddIndex(tableName, uniqueColumns.Select(c => c.Name), uniqueColumns.Key, true);
            }
        }

        private static IEnumerable<string> GetTableConstraints(IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            if (columns.Any(c => c.IsPrimaryKey))
            {
                yield return string.Format(CultureInfo.InvariantCulture, @" CONSTRAINT ""{0}"" PRIMARY KEY ({1})",
                    primaryKeyConstraintName,
                    string.Join(", ", columns.Where(c => c.IsPrimaryKey).Select(c => "\"" + c.Name + "\"").ToArray()));
            }
        }

        private static string GetColumnDefinition(Column column)
        {
            return string.Format(CultureInfo.InvariantCulture, @"""{0}"" {1}{2}",
                column.Name,
                GetTypeSpecifier(column.DataType),
                GetColumnConstraint(column));
        }

        private static string GetColumnConstraint(Column column)
        {
            CreatedColumn createdColumn = column as CreatedColumn;
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
                createdColumn != null && createdColumn.IsIdentity && !createdColumn.IsPrimaryKey ? "AUTOINCREMENT" : string.Empty, // INTEGER column as PK is automatically AUTOINCREMENT: http://www.sqlite.org/autoinc.html
                createdColumn != null && createdColumn.IsIdentity ? string.Empty : (column.IsNullable ? " NULL" : " NOT NULL"),
                column.DefaultValue != null ? " DEFAULT " + GetDefaultValueAsString(column.DefaultValue) : string.Empty);
        }

        private static string GetDefaultValueAsString(object value)
        {
            if (value is SpecialDefaultValue)
            {
                switch ((SpecialDefaultValue)value)
                {
                    case SpecialDefaultValue.CurrentDateTime:
                        return "(DATETIME('now'))"; // http://stackoverflow.com/questions/200309/sqlite-database-default-time-value-now
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
            }
            if (value is DateTime)
            {
                return "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "'"; // ISO 8601
            }
            return value.ToString();
        }

        public IEnumerable<string> DropTable(string tableName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, @"DROP TABLE ""{0}""", tableName);
        }

        public IEnumerable<string> AddColumn(string tableName, Column column)
        {
            yield return string.Format(CultureInfo.InvariantCulture, @"ALTER TABLE ""{0}"" ADD COLUMN {1}", tableName, GetColumnDefinition(column));
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, @"ALTER TABLE ""{0}"" RENAME TO ""{1}""", oldName, newName);
        }

        public IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            // http://stackoverflow.com/questions/805363/how-do-i-rename-a-column-in-a-sqlite-database-table
            throw new NotSupportedException("Rename the table, create a new table with the correct columns, and copy the contents from the renamed table.");
        }

        public IEnumerable<string> DropColumn(string tableName, string columnName)
        {
            // http://stackoverflow.com/questions/805363/how-do-i-rename-a-column-in-a-sqlite-database-table
            throw new NotSupportedException("Rename the table, create a new table with the correct columns, and copy the contents from the renamed table.");
        }

        public IEnumerable<string> AlterColumn(string tableName, Column column)
        {
            // http://www.sqlite.org/omitted.html
            throw new NotSupportedException("Rename the table, create a new table with the correct columns, and copy the contents from the renamed table.");
        }

        public IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName)
        {
            yield return AddIndex(tableName, columnNames, indexName, false);
        }

        private static string AddIndex(string tableName, IEnumerable<string> columnNames, string indexName, bool unique)
        {
            return string.Format(CultureInfo.InvariantCulture, @"CREATE{0} INDEX ""{1}"" ON ""{2}"" ({3})",
                unique ? " UNIQUE" : string.Empty,
                indexName,
                tableName,
                string.Join(", ", columnNames.ToArray()));
        }

        public IEnumerable<string> DropIndex(string tableName, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, @"DROP INDEX ""{0}""", indexName);
        }

        public IEnumerable<string> AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName)
        {
            // Do nothing. SQLite only supports foreign keys under special circumstances (see: http://www.sqlite.org/foreignkeys.html).
            // We do not throw a NotSupportedException since not having foreign keys does not change anything about how the database is used.
            // Therefore, we do not want to force clients to branch migration logic for SQLite.
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> DropForeignKey(string tableName, string constraintName)
        {
            return Enumerable.Empty<string>(); // see comments in AddForeignKey
        }

        public IEnumerable<string> DropPrimaryKey(string tableName, string constraintName)
        {
            // http://stackoverflow.com/questions/849269/sqlite-how-to-remove-an-unamed-primary-key
            throw new NotSupportedException("Primary keys cannot be added/removed retrospectively. If you need a different primary key, you need to recreate the table with the right primary key and copy the contents from the old table.");
        }

        public IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            // http://stackoverflow.com/questions/946011/sqlite-add-primary-key
            throw new NotSupportedException("Primary keys cannot be added/removed retrospectively. If you need a different primary key, you need to recreate the table with the right primary key and copy the contents from the old table.");
        }

        public IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            yield return AddIndex(tableName, columnNames, constraintName, true);
        }

        public IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName)
        {
            return DropIndex(tableName, constraintName);
        }

        public IEnumerable<string> DropDefault(string tableName, Column column)
        {
            // http://www.sqlite.org/omitted.html
            throw new NotSupportedException("Rename the table, create a new table with the correct columns, and copy the contents from the renamed table.");
        }

        private static string GetTypeSpecifier(DataType type)
        {
            switch (type.DbType)
            {
                case DbType.AnsiString:
                    return "TEXT";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Byte:
                    return "INTEGER";
                case DbType.Boolean:
                    return "INTEGER";
                    //case DbType.Currency:
                    //    break;
                case DbType.Date:
                    return "DATETIME";
                case DbType.DateTime:
                    return "DATETIME";
                case DbType.Decimal:
                    return "NUMERIC";
                case DbType.Double:
                    return "NUMERIC";
                case DbType.Guid:
                    return "UNIQUEIDENTIFIER";
                case DbType.Int16:
                    return "INTEGER";
                case DbType.Int32:
                    return "INTEGER";
                case DbType.Int64:
                    return "INTEGER";
                    //case DbType.Object:
                    //    break;
                case DbType.SByte:
                    return "INTEGER";
                case DbType.Single:
                    return "NUMERIC";
                case DbType.String:
                    return "TEXT";
                case DbType.Time:
                    return "DATETIME";
                case DbType.UInt16:
                    return "INTEGER";
                case DbType.UInt32:
                    return "INTEGER";
                case DbType.UInt64:
                    return "INTEGER";
                case DbType.VarNumeric:
                    return "NUMERIC";
                case DbType.AnsiStringFixedLength:
                    return "TEXT";
                case DbType.StringFixedLength:
                    return "TEXT";
                    //case DbType.Xml:
                    //    break;
                    //case DbType.DateTime2:
                    //    break;
                    //case DbType.DateTimeOffset:
                    //    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}