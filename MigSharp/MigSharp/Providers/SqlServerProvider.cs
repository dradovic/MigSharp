using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace MigSharp.Providers
{
    internal class SqlServerProvider : IProvider
    {
        private const string Identation = "\t";

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns)
        {
            List<string> primaryKeyColumns = new List<string>();
            string commandText = string.Format(@"{0}({1}", CreateTable(tableName), Environment.NewLine);
            bool columnDelimiterIsNeeded = false;
            foreach (CreatedColumn column in columns)
            {
                if (columnDelimiterIsNeeded) commandText += string.Format(",{0}", Environment.NewLine);

                if (column.IsPrimaryKey)
                {
                    primaryKeyColumns.Add(column.Name);
                }

                commandText += string.Format("{0}{1} {2} {3}NULL", 
                    Identation, 
                    Escape(column.Name),
                    GetTypeSpecifier(column.Type),
                    column.IsNullable ? string.Empty : "NOT ");

                columnDelimiterIsNeeded = true;
            }

            if (primaryKeyColumns.Count > 0)
            {
                // TODO: make clustering configurable
                commandText += string.Format(",{0} CONSTRAINT [PK_{1}] PRIMARY KEY {2}",
                    Environment.NewLine,
                    EscapeAsNamePart(tableName), 
                    Environment.NewLine);
                commandText += string.Format("({0}", Environment.NewLine);

                columnDelimiterIsNeeded = false;
                foreach (string column in primaryKeyColumns)
                {
                    if (columnDelimiterIsNeeded) commandText += string.Format(",{0}", Environment.NewLine);

                    // TODO: make sort order configurable
                    commandText += string.Format("{0}{1}", Identation, Escape(column));

                    columnDelimiterIsNeeded = true;
                }
                commandText += string.Format("{0})WITH (IGNORE_DUP_KEY = OFF)", Environment.NewLine);
            }

            commandText += Environment.NewLine;
            commandText += string.Format("){0}", Environment.NewLine);
            yield return commandText;
        }

        public IEnumerable<string> AddColumns(string tableName, IEnumerable<AddedColumn> columns)
        {
            Debug.Assert(columns.Count() > 0);

            // assemble ALTER TABLE statements
            foreach (AddedColumn column in columns)
            {
                string commandText = string.Format(@"{0} ADD ", AlterTable(tableName));

                string defaultConstraintClause = string.Empty;
                if (column.DefaultValue != null)
                {
                    string defaultConstraint = GetDefaultConstraintName(tableName, column.Name);
                    defaultConstraintClause = string.Format(" CONSTRAINT {0}  DEFAULT {1}", defaultConstraint, column.DefaultValue);
                }
                commandText += string.Format("{0} {1} {2}NULL{3}",
                    Escape(column.Name), 
                    GetTypeSpecifier(column.Type), 
                    column.IsNullable ? string.Empty : "NOT ",
                    defaultConstraintClause);

                yield return commandText;
            }

            // add commands to drop default constraints
            foreach (AddedColumn column in columns.Where(c => (c.Options & AddColumnOptions.DropDefaultAfterCreation) != 0))
            {
                foreach (string commandText in DropDefaultConstraint(tableName, column.Name))
                {
                    yield return commandText;
                }
            }
        }

        private static string GetDefaultConstraintName(string tableName, string columnName)
        {
            return string.Format("[DF_{0}_{1}]", EscapeAsNamePart(tableName), EscapeAsNamePart(columnName));
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            yield return string.Format("EXEC dbo.sp_rename @objname = N'[dbo].{0}', @newname = N'{1}', @objtype = N'OBJECT'", Escape(oldName), newName);
        }

        public IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            yield return string.Format("EXEC dbo.sp_rename @objname=N'[dbo].{0}.{1}', @newname=N'{2}', @objtype=N'COLUMN'", Escape(tableName), Escape(oldName), newName);
        }

        public IEnumerable<string> DropDefaultConstraint(string tableName, string columnName)
        {
            yield return string.Format("{0} DROP CONSTRAINT {1}", AlterTable(tableName), GetDefaultConstraintName(tableName, columnName));
        }

        private static string CreateTable(string tableName)
        {
            return string.Format("CREATE TABLE [dbo].{0}", Escape(tableName));
        }

        private static string AlterTable(string tableName)
        {
            return string.Format("ALTER TABLE [dbo].{0}", Escape(tableName));
        }

        private static string Escape(string name)
        {
            return string.Format("[{0}]", name);
        }

        private static string EscapeAsNamePart(string name)
        {
            return name; // TODO: provide a real implementation
        }

        private static string GetTypeSpecifier(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                    break;
                case DbType.Binary:
                    break;
                case DbType.Byte:
                    return "[smallint]";
                case DbType.Boolean:
                    break;
                case DbType.Currency:
                    break;
                case DbType.Date:
                    break;
                case DbType.DateTime:
                    break;
                case DbType.Decimal:
                    break;
                case DbType.Double:
                    break;
                case DbType.Guid:
                    break;
                case DbType.Int16:
                    break;
                case DbType.Int32:
                    return "[int]";
                case DbType.Int64:
                    break;
                case DbType.Object:
                    break;
                case DbType.SByte:
                    break;
                case DbType.Single:
                    break;
                case DbType.String:
                    return "[nvarchar](max)";
                case DbType.Time:
                    break;
                case DbType.UInt16:
                    break;
                case DbType.UInt32:
                    break;
                case DbType.UInt64:
                    break;
                case DbType.VarNumeric:
                    break;
                case DbType.AnsiStringFixedLength:
                    break;
                case DbType.StringFixedLength:
                    break;
                case DbType.Xml:
                    break;
                case DbType.DateTime2:
                    break;
                case DbType.DateTimeOffset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            throw new NotImplementedException();
        }
    }
}