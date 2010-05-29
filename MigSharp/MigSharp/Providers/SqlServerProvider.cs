using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

using MigSharp.Core;

namespace MigSharp.Providers
{
    internal class SqlServerProvider : IProvider
    {
        private const string Identation = "  ";

        public IEnumerable<string> AddColumns(string tableName, IEnumerable<AddedColumn> columns)
        {
            Debug.Assert(columns.Count() > 0);

            // assemble ALTER TABLE statement
            List<string> defaultConstraintsToDrop = new List<string>();
            bool columnDelimiterIsNeeded = false;
            string commandText = string.Format(@"{0} ADD{1}", AlterTable(tableName), Environment.NewLine);
            foreach (AddedColumn column in columns)
            {
                if (columnDelimiterIsNeeded) commandText += string.Format(",{0}", Environment.NewLine);

                string defaultConstraintClause = string.Empty;
                if (column.DefaultValue != null)
                {
                    string defaultConstraint = string.Format("DF_{0}_{1}", EscapeAsNamePart(tableName), EscapeAsNamePart(column.Name));
                    defaultConstraintClause = string.Format(" CONSTRAINT {0} DEFAULT {1}", defaultConstraint, column.DefaultValue);
                    if ((column.Options | AddColumnOptions.DropDefaultAfterCreation) != 0)
                    {
                        defaultConstraintsToDrop.Add(defaultConstraint);
                    }
                }
                commandText += string.Format("{0}{1} {2} {3}NULL{4}",
                    Identation,
                    Escape(column.Name), 
                    GetTypeSpecifier(column.Type), 
                    column.IsNullable ? string.Empty : "NOT ", defaultConstraintClause);
                columnDelimiterIsNeeded = true;
            }
            IEnumerable<string> commandTexts = new[] { commandText };

            // add commands to drop default constraints
            foreach (string defaultConstraint in defaultConstraintsToDrop)
            {
                commandTexts = commandTexts.Concat(DropDefaultConstraint(tableName, defaultConstraint));
            }

            return commandTexts;
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            yield return string.Format("sp_rename N'{0}', N'{1}'", oldName, newName);
        }

        public IEnumerable<string> RenameColumn(string oldName, string newName)
        {
            yield return string.Format("sp_rename N'{0}', N'{1}', 'COLUMN'", oldName, newName);
        }

        public IEnumerable<string> DropDefaultConstraint(string tableName, string constraintName)
        {
            yield return string.Format(string.Format("{0} DROP CONSTRAINT {1}", AlterTable(tableName), constraintName));
        }

        private static string AlterTable(string tableName)
        {
            return string.Format("ALTER TABLE dbo.{0}", Escape(tableName));
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
                    return "SMALLINT";
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
                    return "INT";
                case DbType.Int64:
                    break;
                case DbType.Object:
                    break;
                case DbType.SByte:
                    break;
                case DbType.Single:
                    break;
                case DbType.String:
                    return "NVARCHAR(MAX)";
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