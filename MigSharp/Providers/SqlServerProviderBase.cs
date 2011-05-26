using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MigSharp.Providers
{
    internal abstract class SqlServerProviderBase : IProvider
    {
        public const int MaximumDbObjectNameLength = 128;

        private const string Identation = "\t";

        public abstract bool SpecifyWith { get; }
        public abstract string Dbo { get; }

        public abstract string ExistsTable(string databaseName, string tableName);

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            string commandText = string.Empty;
            List<string> primaryKeyColumns = new List<string>();
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
                commandText += string.Format(CultureInfo.InvariantCulture, ",{0} CONSTRAINT [{1}] PRIMARY KEY {2}",
                    Environment.NewLine,
                    primaryKeyConstraintName,
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
                commandText += string.Format(CultureInfo.InvariantCulture, "{0}){1}", Environment.NewLine, SpecifyWith ? "WITH (IGNORE_DUP_KEY = OFF)" : string.Empty);
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
                commandText += string.Format(CultureInfo.InvariantCulture, "{0}){1}", Environment.NewLine, SpecifyWith ? "WITH (IGNORE_DUP_KEY = OFF)" : string.Empty);
            }

            commandText += Environment.NewLine;
            commandText += string.Format(CultureInfo.InvariantCulture, "){0}", Environment.NewLine);

            yield return commandText;
        }

        public IEnumerable<string> DropTable(string tableName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP TABLE {0}{1}", Dbo, Escape(tableName));
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

        protected abstract IEnumerable<string> DropDefaultConstraint(string tableName, Column column, bool checkIfExists);

        private static string GetDefaultConstraintClause(string tableName, string columnName, object value)
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

        private static string GetDefaultValueAsString(object value)
        {
            if (value is SpecialDefaultValue)
            {
                switch((SpecialDefaultValue)value)
                {
                    case SpecialDefaultValue.CurrentDateTime:
                        return "GETDATE()";
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

        public abstract IEnumerable<string> RenameTable(string oldName, string newName);

        public abstract IEnumerable<string> RenameColumn(string tableName, string oldName, string newName);

        public IEnumerable<string> DropColumn(string tableName, string columnName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0}{1} DROP COLUMN {2}", Dbo, Escape(tableName), Escape(columnName));
        }

        public IEnumerable<string> AlterColumn(string tableName, Column column)
        {
            // remove any existing default value constraints (before possibly adding new ones)
            foreach (string text in DropDefaultConstraint(tableName, column, true))
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
            yield return string.Format(CultureInfo.InvariantCulture, "CREATE INDEX {0} ON {4}{1} {2}({2}\t{3}{2}){5}",
                Escape(indexName),
                Escape(tableName),
                Environment.NewLine,
                string.Join(string.Format(CultureInfo.InvariantCulture, ",{0}\t", Environment.NewLine), columnNames.Select(Escape).ToArray()),
                Dbo,
                SpecifyWith ? "WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)" : string.Empty);
        }

        public abstract IEnumerable<string> DropIndex(string tableName, string indexName);

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

        public IEnumerable<string> DropPrimaryKey(string tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, columnNames, "PRIMARY KEY");
        }

        public IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, columnNames, "UNIQUE");
        }

        private IEnumerable<string> AddConstraint(string tableName, string constraintName, IEnumerable<string> columnNames, string constraintType)
        {
            yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " ADD  CONSTRAINT [{0}] {3} {1}({1}\t{2}{1}){4}",
                constraintName,
                Environment.NewLine,
                string.Join("," + Environment.NewLine + "\t", columnNames.Select(Escape).ToArray()),
                constraintType,
                SpecifyWith ? "WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF)" : string.Empty);
        }

        public IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> DropDefault(string tableName, Column column)
        {
            Debug.Assert(column.DefaultValue == null);
            return DropDefaultConstraint(tableName, column, false);
        }

        protected string DropConstraint(string tableName, string constraintName)
        {
            return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " DROP CONSTRAINT [{0}]", constraintName);
        }

        private string CreateTable(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}{1}", Dbo, Escape(tableName));
        }

        private string AlterTable(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0}{1}", Dbo, Escape(tableName));
        }

        protected static string Escape(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}]", name);
        }

        protected abstract string GetTypeSpecifier(DataType dataType);
    }
}