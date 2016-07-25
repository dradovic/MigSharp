using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using MigSharp.Core;

namespace MigSharp.Providers
{
    [ProviderExport(Platform.MySql, 5, InvariantName, MaximumDbObjectNameLength = MaximumDbObjectNameLength, EnableAnsiQuotesCommand = "SET SESSION sql_mode='ANSI_QUOTES'", PrefixUnicodeLiterals = PrefixUnicodeLiterals)]
    [Supports(DbType.AnsiString, MaximumSize = 65535, CanBeUsedAsPrimaryKey = true)] // maximum size 65,535 started in MySql 5.0.3 according to http://dev.mysql.com/doc/refman/5.0/en/char.html
    [Supports(DbType.AnsiString)] // translates to LONGTEXT without specifying the size
    [Supports(DbType.Binary)]
    [Supports(DbType.Byte, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Boolean, CanBeUsedAsPrimaryKey = true, Warning = "Requires custom ADO.NET code to convert to/from an Int32 (using System.Convert).")]
    [Supports(DbType.DateTime, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Decimal, MaximumSize = 28, MaximumScale = 28, CanBeUsedAsPrimaryKey = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Decimal, MaximumSize = 28, CanBeUsedAsPrimaryKey = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Double)]
    [Supports(DbType.Int16, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int32, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int64, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Single, CanBeUsedAsPrimaryKey = true, Warning = "Using DbType.Single might give you some unexpected problems because all calculations in MySQL are done with double precision.")]
    [Supports(DbType.String, MaximumSize = 65535, CanBeUsedAsPrimaryKey = true)] // maximum size 65,535 started in MySql 5.0.3 according to http://dev.mysql.com/doc/refman/5.0/en/char.html
    [Supports(DbType.String)] // translates to LONGTEXT without specifying the Size
    [Supports(DbType.Time, Warning = "Fractional seconds are not stored by MySQL until version 5.6.4")] //http://stackoverflow.com/questions/2572209/why-doesnt-mysql-support-millisecond-microsecond-precision
    [Supports(DbType.UInt16)]
    [Supports(DbType.UInt32)]
    [Supports(DbType.UInt64)]
    [Supports(DbType.AnsiStringFixedLength, MaximumSize = 255, CanBeUsedAsPrimaryKey = true)] // http://dev.mysql.com/doc/refman/5.0/en/char.html
    [Supports(DbType.StringFixedLength, MaximumSize = 255, CanBeUsedAsPrimaryKey = true)] // http://dev.mysql.com/doc/refman/5.0/en/char.html
    internal class MySqlProvider : IProvider
    {
        public const string InvariantName = "MySql.Data.MySqlClient";

        private const string Identation = "\t";

        private const int MaximumDbObjectNameLength = 64; // http://stackoverflow.com/questions/6868302/maximum-length-of-a-table-name-in-mysql

        private const bool PrefixUnicodeLiterals = true;

        public string ExistsTable(string databaseName, TableName tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(TABLE_NAME)
                FROM information_schema.tables
                WHERE table_schema = '{0}'
                AND table_name = '{1}';", databaseName, tableName.Name);
        }

        public string ConvertToSql(object value, DbType targetDbType)
        {
            return SqlScriptingHelper.ToSql(value, targetDbType, PrefixUnicodeLiterals);
        }

        public IEnumerable<string> CreateTable(TableName tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            string commandText = string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}", Escape(tableName.Name));

            commandText += string.Format(CultureInfo.InvariantCulture, "({0}", Environment.NewLine);

            bool columnDelimiterIsNeeded = false;
            var primaryKeyColumns = new List<string>();
            foreach (CreatedColumn column in columns)
            {
                if (columnDelimiterIsNeeded) commandText += string.Format(CultureInfo.InvariantCulture, ",{0}", Environment.NewLine);

                if (column.IsPrimaryKey)
                {
                    primaryKeyColumns.Add(column.Name);
                }

                string defaultValue = GetDefaultValueAsString(column.DefaultValue);

                commandText += string.Format(CultureInfo.InvariantCulture, "{0}{1} {2} {3} {4}NULL {5}",
                                             Identation,
                                             Escape(column.Name),
                                             GetTypeSpecifier(column.DataType),
                                             column.IsIdentity ? "AUTO_INCREMENT " : string.Empty,
                                             column.IsNullable ? string.Empty : "NOT ",
                                             defaultValue.Length > 0 ? "DEFAULT " + defaultValue : string.Empty);

                columnDelimiterIsNeeded = true;
            }

            if (primaryKeyColumns.Count > 0)
            {
                commandText += string.Format(CultureInfo.InvariantCulture, ",{0}", Environment.NewLine);
                commandText += "PRIMARY KEY ";
                commandText += string.Format(CultureInfo.InvariantCulture, "({0}", Environment.NewLine);
                columnDelimiterIsNeeded = false;
                foreach (string column in primaryKeyColumns)
                {
                    if (columnDelimiterIsNeeded) commandText += string.Format(CultureInfo.InvariantCulture, ",{0}", Environment.NewLine);

                    // FEATURE: make sort order configurable
                    commandText += string.Format(CultureInfo.InvariantCulture, "{0}{1}", Identation, Escape(column));

                    columnDelimiterIsNeeded = true;
                }
                commandText += string.Format(CultureInfo.InvariantCulture, "){0}", Environment.NewLine);
            }

            foreach (var uniqueColumns in columns
                .Where(c => !string.IsNullOrEmpty(c.UniqueConstraint))
                .GroupBy(c => c.UniqueConstraint))
            {
                commandText += string.Format(CultureInfo.InvariantCulture, ",{0} UNIQUE KEY {1} {2}",
                                             Environment.NewLine,
                                             Escape(uniqueColumns.Key),
                                             Environment.NewLine);

                commandText += string.Format(CultureInfo.InvariantCulture, "({0}", Environment.NewLine);

                columnDelimiterIsNeeded = false;
                foreach (string column in uniqueColumns.Select(c => c.Name))
                {
                    if (columnDelimiterIsNeeded) commandText += string.Format(CultureInfo.InvariantCulture, ",{0}", Environment.NewLine);
                    commandText += string.Format(CultureInfo.InvariantCulture, "{0}{1}", Identation, Escape(column));
                    columnDelimiterIsNeeded = true;
                }
                commandText += string.Format(CultureInfo.InvariantCulture, "{0})", Environment.NewLine);
            }

            commandText += string.Format(CultureInfo.InvariantCulture, "){0}", Environment.NewLine);

            yield return commandText;
        }

        private string GetDefaultValueAsString(object value)
        {
            if (value is SpecialDefaultValue)
            {
                switch ((SpecialDefaultValue)value)
                {
                    case SpecialDefaultValue.CurrentDateTime:
                        return "NOW()"; // works as of MySQL 5.6 (see: http://stackoverflow.com/questions/5818423/set-now-as-default-value-for-datetime-datatype)
                    case SpecialDefaultValue.CurrentUtcDateTime:
                        return "NOW()"; // requires to set the timezone of the MySQL server to GMT (see: http://stackoverflow.com/a/19075291/331281)
                    default:
                        throw new ArgumentOutOfRangeException("value", "Invalid special default value");
                }
            }
            else if (value is DateTime)
            {
                return ConvertToSql(value, DbType.DateTime);
            }
            else if (value is string)
            {
                return ConvertToSql(value, DbType.String);
            }
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }


        public IEnumerable<string> DropTable(TableName tableName, bool checkIfExists)
        {
            yield return string.Format(CultureInfo.InvariantCulture, @"DROP TABLE {0} {1}", checkIfExists ? "IF EXISTS " : string.Empty, Escape(tableName.Name));
        }

        public IEnumerable<string> AddColumn(TableName tableName, Column column)
        {
            string commandText = string.Format(CultureInfo.InvariantCulture, @"ALTER TABLE {0} ADD {1}",
                                               Escape(tableName.Name),
                                               GetColumnSpec(column));
            yield return commandText;
        }

        private string GetColumnSpec(Column column)
        {
            string defaultValue = GetDefaultValueAsString(column.DefaultValue);

            return string.Format(CultureInfo.InvariantCulture, @"{0} {1} {2}NULL {3}",
                                 Escape(column.Name),
                                 GetTypeSpecifier(column.DataType),
                                 column.IsNullable ? string.Empty : "NOT ",
                                 defaultValue.Length > 0 ? "DEFAULT " + defaultValue : string.Empty);
        }

        public IEnumerable<string> RenameTable(TableName oldName, string newName)
        {
            yield return string.Format("RENAME TABLE {0} TO {1}", Escape(oldName.Name), Escape(newName));
        }

        public IEnumerable<string> RenameColumn(TableName tableName, string oldName, string newName)
        {
            // MySQL requires the full column definition (including data type, nullability, default value) for column rename
            // (see: http://stackoverflow.com/questions/8553130/rename-a-column-in-mysql-table-without-having-to-repeat-its-type-definition).
            // The set of commands works but requires "Allow User Variables=True" in the connection string and it loses
            // all extra column definitions.
//            yield return string.Format(CultureInfo.InvariantCulture, @"SELECT @DB := DATABASE();
//SELECT @CTYPE := COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @DB AND TABLE_NAME = '{3}' AND COLUMN_NAME = '{4}';
//SET @STMT = CONCAT('ALTER TABLE {0} CHANGE {1} {2} ', @CTYPE);
//PREPARE REN_STMT FROM @STMT;
//EXECUTE REN_STMT;
//DEALLOCATE PREPARE REN_STMT;", Escape(tableName), Escape(oldName), Escape(newName), tableName, oldName);

            throw new NotSupportedException("Rename column is not supported by the MySQL provider.");
        }

        public virtual IEnumerable<string> DropColumn(TableName tableName, string columnName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP COLUMN {1}",
                                       Escape(tableName.Name),
                                       Escape(columnName));
        }

        public IEnumerable<string> AlterColumn(TableName tableName, Column column)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} MODIFY COLUMN {1}",
                                       Escape(tableName.Name),
                                       GetColumnSpec(column));
        }

        public IEnumerable<string> DropDefault(TableName tableName, Column column)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
                                       Escape(tableName.Name),
                                       Escape(column.Name));
        }

        public IEnumerable<string> CreateSchema(string schemaName)
        {
            throw new NotSupportedException("Schemata are not supported. On MySQL, the schema is an equivalent of the database.");
        }

        public IEnumerable<string> DropSchema(string schemaName)
        {
            throw new NotSupportedException("Schemata are not supported. On MySQL, the schema is an equivalent of the database.");
        }

        public IEnumerable<string> AddIndex(TableName tableName, IEnumerable<string> columnNames, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "CREATE INDEX {0} ON {1} ({2})",
                                       Escape(indexName),
                                       Escape(tableName.Name),
                                       string.Join(", ", columnNames.Select(Escape).ToArray()));
        }

        public IEnumerable<string> DropIndex(TableName tableName, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP INDEX {0} ON {1}",
                                       Escape(indexName),
                                       Escape(tableName.Name));
        }

        public IEnumerable<string> AddForeignKey(TableName tableName, TableName referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName, bool cascadeOnDelete)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) {3} REFERENCES {4} ({5}) {6}",
                                       Escape(tableName.Name),
                                       Escape(constraintName),
                                       string.Join(", ", columnNames.Select(n => Escape(n.ColumnName)).ToArray()),
                                       Environment.NewLine,
                                       Escape(referencedTableName.Name),
                                       string.Join(", ", columnNames.Select(n => Escape(n.ReferencedColumnName)).ToArray()),
                                       cascadeOnDelete ? " ON DELETE CASCADE" : string.Empty);
        }

        public IEnumerable<string> DropForeignKey(TableName tableName, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP FOREIGN KEY {1}",
                                       Escape(tableName.Name),
                                       Escape(constraintName));
        }

        public IEnumerable<string> AddPrimaryKey(TableName tableName, IEnumerable<string> columnNames, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} ADD PRIMARY KEY({1})",
                                       Escape(tableName.Name),
                                       string.Join(", ", columnNames.Select(Escape).ToArray()));
        }

        public IEnumerable<string> RenamePrimaryKey(TableName tableName, string oldName, string newName)
        {
            // http://dev.mysql.com/doc/refman/5.0/en/drop-index.html
            throw new NotSupportedException("The primary key name is always PRIMARY.");
        }

        public IEnumerable<string> DropPrimaryKey(TableName tableName, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP PRIMARY KEY",
                                       Escape(tableName.Name));
        }

        public IEnumerable<string> AddUniqueConstraint(TableName tableName, IEnumerable<string> columnNames, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} ADD UNIQUE KEY {1} ({2})",
                                       Escape(tableName.Name),
                                       Escape(constraintName),
                                       string.Join(", ", columnNames.Select(Escape).ToArray()));
        }

        public IEnumerable<string> DropUniqueConstraint(TableName tableName, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP KEY {1}",
                                       Escape(tableName.Name),
                                       Escape(constraintName));
        }

        protected static string Escape(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, "`{0}`", name);
        }

        // see: https://svn.mindtouch.com/source/public/nhibernate/2.1.2/src/NHibernate/Dialect/MySQLDialect.cs
        private static string GetTypeSpecifier(DataType type)
        {
            switch (type.DbType)
            {
                case DbType.AnsiString:
                case DbType.String:
                    if (type.Size.HasValue && type.Size > 0)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "VARCHAR({0})", type.Size);
                    }
                    else
                    {
                        return "LONGTEXT"; // http://stackoverflow.com/questions/7314682/what-is-the-disadvantage-to-using-a-mysql-longtext-sized-field-when-every-entry
                    }
                case DbType.Binary:
                    return "LONGBLOB";
                case DbType.Byte:
                    return "TINYINT UNSIGNED";
                case DbType.Boolean:
                    return "BIT";
                    //case DbType.Currency:
                    //    break;
                case DbType.Date:
                    return "DATE";
                case DbType.DateTime:
                    return "DATETIME";
                case DbType.Decimal:
                    if (type.Scale.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "NUMERIC({0}, {1})", type.Size, type.Scale);
                    }
                    else
                    {
                        return string.Format(CultureInfo.InvariantCulture, "NUMERIC({0})", type.Size);
                    }
                case DbType.Double:
                    return "DOUBLE";
                case DbType.Int16:
                    return "SMALLINT";
                case DbType.Int32:
                    return "INT";
                case DbType.Int64:
                    return "BIGINT";
                    //case DbType.Object:
                    //    break;
                case DbType.SByte:
                    return "TINYINT";
                case DbType.Single:
                    return "FLOAT";
                case DbType.Time:
                    return "TIME";
                case DbType.UInt16:
                    return "SMALLINT UNSIGNED";
                case DbType.UInt32:
                    return "INTEGER UNSIGNED";
                case DbType.UInt64:
                    return "BIGINT UNSIGNED";
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return string.Format(CultureInfo.InvariantCulture, "CHAR({0})", type.Size);
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}