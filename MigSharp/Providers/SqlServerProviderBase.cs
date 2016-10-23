using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using MigSharp.Core;

namespace MigSharp.Providers
{
    internal abstract class SqlServerProviderBase : IProvider
    {
        public const int MaximumDbObjectNameLength = 128;
        protected const bool PrefixUnicodeLiterals = true;

        private const string Identation = "\t";

        public abstract bool SpecifyWith { get; }
        public abstract string GetSchemaPrefix(TableName tableName);

        public string GetTableQualifier(TableName tableName)
        {
            return GetSchemaPrefix(tableName) + Escape(tableName.Name);
        }

        public abstract string ExistsTable(string databaseName, TableName tableName);

        public string ConvertToSql(object value, DbType targetDbType)
        {
            return SqlScriptingHelper.ToSql(value, targetDbType, true);
        }

        public IEnumerable<string> CreateTable(TableName tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
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

                string defaultConstraintClause = GetDefaultConstraintClause(tableName, column.Name, column.DefaultValue, column.DataType);
                commandText += string.Format(CultureInfo.InvariantCulture, "{0}{1} {2} {3}{4}NULL{5}",
                    Identation,
                    Escape(column.Name),
                    GetTypeSpecifier(column.DataType, column.IsRowVersion),
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

        public virtual IEnumerable<string> DropTable(TableName tableName, bool checkIfExists)
        {
            string commandText = string.Format(CultureInfo.InvariantCulture, "DROP TABLE {0}", GetTableQualifier(tableName));
            if (checkIfExists)
            {
                yield return "IF (" + ExistsTable(null, tableName) + ") != 0 BEGIN " + commandText + " END";
            }
            else
            {
                yield return commandText;
            }
        }

        public IEnumerable<string> AddColumn(TableName tableName, Column column)
        {
            // assemble ALTER TABLE statements
            string commandText = string.Format(CultureInfo.InvariantCulture, @"{0} ADD ", AlterTable(tableName));
            string defaultConstraintClause = GetDefaultConstraintClause(tableName, column.Name, column.DefaultValue, column.DataType);
            commandText += string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}NULL{3}",
                Escape(column.Name),
                GetTypeSpecifier(column.DataType, column.IsRowVersion),
                column.IsNullable ? string.Empty : "NOT ",
                defaultConstraintClause);
            yield return commandText;
        }

        protected abstract IEnumerable<string> DropDefaultConstraint(TableName tableName, string columnName, bool checkIfExists);

        private string GetDefaultConstraintClause(TableName tableName, string columnName, object value, DataType dataType)
        {
            string constraintName = GetDefaultConstraintName(tableName.Name, columnName);
            string defaultConstraintClause = string.Empty;
            if (value != null)
            {
                string defaultValue = GetDefaultValueAsString(value, dataType);
                defaultConstraintClause = string.Format(CultureInfo.InvariantCulture, " CONSTRAINT [{0}]  DEFAULT {1}", constraintName, defaultValue);
            }
            return defaultConstraintClause;
        }

        protected static string GetDefaultConstraintName(string tableName, string columnName)
        {
            return ObjectNameHelper.GetObjectName(tableName, "DF", MaximumDbObjectNameLength, columnName);
        }

        private string GetDefaultValueAsString(object value, DataType dataType)
        {
            if (value is SpecialDefaultValue)
            {
                switch((SpecialDefaultValue)value)
                {
                    case SpecialDefaultValue.CurrentDateTime:
                        return "GETDATE()";
                    case SpecialDefaultValue.CurrentUtcDateTime:
                        return "GETUTCDATE()";
                    case SpecialDefaultValue.CurrentDateTimeOffset:
                        return "SYSDATETIMEOFFSET()";
                    case SpecialDefaultValue.NewGuid:
                        return "NEWID()";
                    case SpecialDefaultValue.NewSequentialGuid:
                        return "NEWSEQUENTIALID()";
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
            }
            return ConvertToSql(value, dataType.DbType);
        }

        public abstract IEnumerable<string> RenameTable(TableName oldName, string newName);

        public abstract IEnumerable<string> RenameColumn(TableName tableName, string oldName, string newName);

        public virtual IEnumerable<string> DropColumn(TableName tableName, string columnName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP COLUMN {1}", GetTableQualifier(tableName), Escape(columnName));
        }

        public IEnumerable<string> AlterColumn(TableName tableName, Column column)
        {
            // remove any existing default value constraints (before possibly adding new ones)
            foreach (string text in DropDefaultConstraint(tableName, column.Name, true))
            {
                yield return text;
            }
            yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " ALTER COLUMN [{0}] {1} {2}NULL",
                column.Name,
                GetTypeSpecifier(column.DataType, column.IsRowVersion),
                column.IsNullable ? string.Empty : "NOT ");
            if (column.DefaultValue != null)
            {
                yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " ADD {0} FOR {1}",
                    GetDefaultConstraintClause(tableName, column.Name, column.DefaultValue, column.DataType),
                    Escape(column.Name));
            }
        }

        public IEnumerable<string> AddIndex(TableName tableName, IEnumerable<string> columnNames, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "CREATE INDEX {0} ON {1} {2}({2}\t{3}{2}){4}",
                Escape(indexName),
                GetTableQualifier(tableName),
                Environment.NewLine,
                string.Join(string.Format(CultureInfo.InvariantCulture, ",{0}\t", Environment.NewLine), columnNames.Select(Escape).ToArray()),
                SpecifyWith ? "WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF)" : string.Empty);
        }

        public abstract IEnumerable<string> DropIndex(TableName tableName, string indexName);

        public IEnumerable<string> AddForeignKey(TableName tableName, TableName referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName, bool cascadeOnDelete)
        {
            yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, "  ADD  CONSTRAINT [{0}] FOREIGN KEY({1}){2}REFERENCES {3} ({4}){5}",
                constraintName,
                string.Join(", ", columnNames.Select(n => Escape(n.ColumnName)).ToArray()),
                Environment.NewLine,
                GetTableQualifier(referencedTableName),
                string.Join(", ", columnNames.Select(n => Escape(n.ReferencedColumnName)).ToArray()),
                cascadeOnDelete ? " ON DELETE CASCADE" : string.Empty);
        }

        public IEnumerable<string> DropForeignKey(TableName tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> AddPrimaryKey(TableName tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, columnNames, "PRIMARY KEY");
        }

        public abstract IEnumerable<string> RenamePrimaryKey(TableName tableName, string oldName, string newName);

        public IEnumerable<string> DropPrimaryKey(TableName tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> AddUniqueConstraint(TableName tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, columnNames, "UNIQUE");
        }

        private IEnumerable<string> AddConstraint(TableName tableName, string constraintName, IEnumerable<string> columnNames, string constraintType)
        {
            yield return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " ADD  CONSTRAINT [{0}] {3} {1}({1}\t{2}{1}){4}",
                constraintName,
                Environment.NewLine,
                string.Join("," + Environment.NewLine + "\t", columnNames.Select(Escape).ToArray()),
                constraintType,
                SpecifyWith ? "WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF)" : string.Empty);
        }

        public IEnumerable<string> DropUniqueConstraint(TableName tableName, string constraintName)
        {
            yield return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> DropDefault(TableName tableName, Column column)
        {
            Debug.Assert(column.DefaultValue == null);
            return DropDefaultConstraint(tableName, column.Name, false);
        }

        public abstract IEnumerable<string> CreateSchema(string schemaName);

        public abstract IEnumerable<string> DropSchema(string schemaName);

        protected string DropConstraint(TableName tableName, string constraintName)
        {
            return AlterTable(tableName) + string.Format(CultureInfo.InvariantCulture, " DROP CONSTRAINT [{0}]", constraintName);
        }

        private string CreateTable(TableName tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}", GetTableQualifier(tableName));
        }

        private string AlterTable(TableName tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0}", GetTableQualifier(tableName));
        }

        protected static string Escape(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}]", name);
        }

        protected abstract string GetTypeSpecifier(DataType dataType, bool isRowVersion);
    }
}