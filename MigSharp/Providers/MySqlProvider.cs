using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.MySqlExperimental, InvariantName, MaximumDbObjectNameLength = MaximumDbObjectNameLength, EnableAnsiQuotesCommand = "SET SESSION sql_mode='ANSI_QUOTES'")]
    [Supports(DbType.AnsiString, MaximumSize = 65535, CanBeUsedAsPrimaryKey = true)] // maximum size 65,535 started in MySql 5.0.3 according to http://dev.mysql.com/doc/refman/5.0/en/char.html
    [Supports(DbType.AnsiString)] // translates to LONGTEXT without specifying the size
    [Supports(DbType.Binary)]
    [Supports(DbType.Byte, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Boolean, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.DateTime, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Decimal, MaximumSize = 28, MaximumScale = 28, CanBeUsedAsPrimaryKey = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Decimal, MaximumSize = 28, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Double)]
    [Supports(DbType.Int16, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int32, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int64, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Single, CanBeUsedAsPrimaryKey = true, Warning = "Using DbType.Single might give you some unexpected problems because all calculations in MySQL are done with double precision.")]
    [Supports(DbType.String, MaximumSize = 65535, CanBeUsedAsPrimaryKey = true)] // maximum size 65,535 started in MySql 5.0.3 according to http://dev.mysql.com/doc/refman/5.0/en/char.html
    [Supports(DbType.String)] // translates to LONGTEXT without specifying the Size
    [Supports(DbType.Time)]
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

        public string ExistsTable(string databaseName, string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"SELECT 1
                FROM information_schema.tables
                WHERE table_schema = '{0}'
                AND table_name = '{1}';", databaseName, tableName);
            //return string.Format("IF EXISTS (SHOW TABLES LIKE '{0}') THEN RETURN 1; ELSE RETURN 0; END IF", tableName);
        }

        public string ConvertToSql(object value, DbType targetDbType)
        {
            return SqlScriptingHelper.ToSql(value, targetDbType);
        }

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            string commandText = string.Empty;
            var primaryKeyColumns = new List<string>();
            commandText += string.Format(CultureInfo.InvariantCulture, @"{0}({1}", CreateTable(tableName), Environment.NewLine);
            bool columnDelimiterIsNeeded = false;
            foreach (CreatedColumn column in columns)
            {
                if (columnDelimiterIsNeeded) commandText += string.Format(CultureInfo.InvariantCulture, ",{0}", Environment.NewLine);

                if (column.IsPrimaryKey)
                {
                    primaryKeyColumns.Add(column.Name);
                }

                string defaultConstraintClause = GetDefaultConstraintClause(tableName, column.Name, column.DefaultValue);
                commandText += string.Format(CultureInfo.InvariantCulture, "{0}{1} {2} {3}{4}NULL{5}",
                    Identation,
                    Escape(column.Name),
                    GetTypeSpecifier(column.DataType),
                    column.IsIdentity ? "IDENTITY " : string.Empty,
                    column.IsNullable ? string.Empty : "NOT ",
                    defaultConstraintClause);

                columnDelimiterIsNeeded = true;
            }

            if (primaryKeyColumns.Count > 0)
            {
                // FEATURE: support clustering
                commandText += string.Format(CultureInfo.InvariantCulture, ",{0} PRIMARY KEY ",
                    Environment.NewLine);
                commandText += string.Format(CultureInfo.InvariantCulture, "({0}", Environment.NewLine);

                columnDelimiterIsNeeded = false;
                foreach (string column in primaryKeyColumns)
                {
                    if (columnDelimiterIsNeeded) commandText += string.Format(CultureInfo.InvariantCulture, ",{0}", Environment.NewLine);

                    // FEATURE: make sort order configurable
                    commandText += string.Format(CultureInfo.InvariantCulture, "{0}{1}", Identation, Escape(column));

                    columnDelimiterIsNeeded = true;
                }
                commandText += string.Format(CultureInfo.InvariantCulture, "{0})", Environment.NewLine);
            }

            foreach (var uniqueColumns in columns
                .Where(c => !string.IsNullOrEmpty(c.UniqueConstraint))
                .GroupBy(c => c.UniqueConstraint))
            {
                commandText += string.Format(CultureInfo.InvariantCulture, ",{0} CONSTRAINT [{1}] UNIQUE {2}",
                    Environment.NewLine,
                    uniqueColumns.Key,
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

            commandText += Environment.NewLine;
            commandText += string.Format(CultureInfo.InvariantCulture, "){0}", Environment.NewLine);

            yield return commandText;
        }

        public IEnumerable<string> DropTable(string tableName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP TABLE {0}", Escape(tableName));
        }

        public IEnumerable<string> AddColumn(string tableName, Column column)
        {
            // assemble ALTER TABLE statements
            string commandText = string.Format(CultureInfo.InvariantCulture, @"{0} ADD ", AlterTable(tableName));
            string defaultConstraintClause = GetDefaultConstraintClause(tableName, column.Name, column.DefaultValue);
            commandText += string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}NULL{3}",
                Escape(column.Name),
                GetTypeSpecifier(column.DataType),
                column.IsNullable ? string.Empty : "NOT ",
                defaultConstraintClause);
            yield return commandText;
        }

        private IEnumerable<string> DropDefaultConstraint(string tableName, string columnName, bool checkIfExists)
        {
            string constraintName = GetDefaultConstraintName(tableName, columnName);
            string commandText = DropConstraint(tableName, constraintName);
            if (checkIfExists)
            {
                commandText = PrefixIfObjectExists(constraintName, commandText);
            }
            yield return commandText;
        }

        private static string PrefixIfObjectExists(string objectName, string commandTextToBePrefixed)
        {
            return string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID('{0}') IS NOT NULL ", objectName) + commandTextToBePrefixed;
        }

        private string GetDefaultConstraintClause(string tableName, string columnName, object value)
        {
            string constraintName = GetDefaultConstraintName(tableName, columnName);
            string defaultConstraintClause = string.Empty;
            if (value != null)
            {
                string defaultValue = GetDefaultValueAsString(value);
                defaultConstraintClause = string.Format(CultureInfo.InvariantCulture, " CONSTRAINT [{0}]  DEFAULT {1}", constraintName, defaultValue);
            }
            return defaultConstraintClause;
        }

        protected static string GetDefaultConstraintName(string tableName, string columnName)
        {
            return ObjectNameHelper.GetObjectName(tableName, "DF", MaximumDbObjectNameLength, columnName);
        }

        private string GetDefaultValueAsString(object value)
        {
            if (value is SpecialDefaultValue)
            {
                switch ((SpecialDefaultValue)value)
                {
                    case SpecialDefaultValue.CurrentDateTime:
                        return "GETDATE()";
                    default:
                        throw new ArgumentOutOfRangeException("value");
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

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            yield return string.Format("EXEC dbo.sp_rename @objname = N'[dbo].{0}', @newname = N'{1}', @objtype = N'OBJECT'", Escape(oldName), newName);            
        }

        public IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            string defaultConstraintName = GetDefaultConstraintName(tableName, oldName);
            string renameDefaultConstraintName = string.Format("EXEC dbo.sp_rename @objname = N'{0}', @newname = N'{1}', @objtype = N'OBJECT'", Escape(defaultConstraintName), GetDefaultConstraintName(tableName, newName));
            yield return PrefixIfObjectExists(defaultConstraintName, renameDefaultConstraintName);
            yield return string.Format("EXEC dbo.sp_rename @objname=N'[dbo].{0}.{1}', @newname=N'{2}', @objtype=N'COLUMN'", Escape(tableName), Escape(oldName), newName);

        }

        public virtual IEnumerable<string> DropColumn(string tableName, string columnName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP COLUMN {1}", Escape(tableName), Escape(columnName));
        }

        public IEnumerable<string> AlterColumn(string tableName, Column column)
        {
            // remove any existing default value constraints (before possibly adding new ones)
            foreach (string text in DropDefaultConstraint(tableName, column.Name, true))
            {
                yield return text;
            }
            yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " ALTER COLUMN [{0}] {1} {2}NULL",
                column.Name,
                GetTypeSpecifier(column.DataType),
                column.IsNullable ? string.Empty : "NOT ");
            if (column.DefaultValue != null)
            {
                yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " ADD {0} FOR {1}",
                    GetDefaultConstraintClause(tableName, column.Name, column.DefaultValue),
                    Escape(column.Name));
            }
        }

        public IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "CREATE INDEX {0} ON {1} {2}({2}\t{3}{2})",
                Escape(indexName),
                Escape(tableName),
                Environment.NewLine,
                string.Join(string.Format(CultureInfo.InvariantCulture, ",{0}\t", Environment.NewLine), columnNames.Select(Escape).ToArray()));
        }

        public IEnumerable<string> DropIndex(string tableName, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP INDEX {0} ON [dbo].{1} WITH ( ONLINE = OFF )", Escape(indexName), Escape(tableName));            
        }

        public IEnumerable<string> AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName)
        {
            yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, "  ADD  CONSTRAINT [{0}] FOREIGN KEY({1}){2}REFERENCES {3} ({4})",
                constraintName,
                string.Join(", ", columnNames.Select(n => Escape(n.ColumnName)).ToArray()),
                Environment.NewLine,
                Escape(referencedTableName),
                string.Join(", ", columnNames.Select(n => Escape(n.ReferencedColumnName)).ToArray()));
        }

        public IEnumerable<string> DropForeignKey(string tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, columnNames, "PRIMARY KEY");
        }

        public IEnumerable<string> RenamePrimaryKey(string tableName, string oldName, string newName)
        {
            yield return string.Format("EXEC sp_rename N'[dbo].{0}.{1}', N'{2}', N'INDEX'", Escape(tableName), Escape(oldName), newName);           
        }

        public IEnumerable<string> DropPrimaryKey(string tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, columnNames, "UNIQUE");
        }

        private static IEnumerable<string> AddConstraint(string tableName, string constraintName, IEnumerable<string> columnNames, string constraintType)
        {
            yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " ADD  CONSTRAINT [{0}] {3} {1}({1}\t{2}{1})",
                constraintName,
                Environment.NewLine,
                string.Join("," + Environment.NewLine + "\t", columnNames.Select(Escape).ToArray()),
                constraintType);
        }

        public IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> DropDefault(string tableName, Column column)
        {
            Debug.Assert(column.DefaultValue == null);
            return DropDefaultConstraint(tableName, column.Name, false);
        }

        protected string DropConstraint(string tableName, string constraintName)
        {
            return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " DROP CONSTRAINT [{0}]", constraintName);
        }

        private static string CreateTable(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}", Escape(tableName));
        }

        private static string AlterTable(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0}", Escape(tableName));
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
                    if (type.Size > 0)
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
                    return string.Format(CultureInfo.InvariantCulture, "NUMERIC({0}, {1})", type.Size, type.Scale);
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