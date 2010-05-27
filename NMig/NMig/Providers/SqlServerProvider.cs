using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace NMig.Providers
{
    internal class SqlServerProvider : IProvider
    {
        public string AddColumns(string tableName, IEnumerable<AddedColumn> columns)
        {
            Debug.Assert(columns.Count() > 0);

            bool columnDelimiterIsNeeded = false;
            string sql = string.Format(@"ALTER TABLE dbo.{0} ADD{1}", Escape(tableName), Environment.NewLine);
            foreach (AddedColumn column in columns)
            {
                if (columnDelimiterIsNeeded) sql += string.Format(",{0}", Environment.NewLine);

                string defaultConstraint = string.Empty;
                if (column.DefaultValue != null)
                {
                    defaultConstraint = string.Format(" CONSTRAINT DF_{0}_{1} DEFAULT {2}", EscapeAsNamePart(tableName), EscapeAsNamePart(column.Name), column.DefaultValue);
                }
                sql += string.Format("  {0} {1} {2}NULL{3}", Escape(column.Name), GetTypeSpecifier(column.Type), column.IsNullable ? string.Empty : "NOT ", defaultConstraint);
                columnDelimiterIsNeeded = true;
            }
            return sql;
        }

        public string RenameTable(string oldName, string newName)
        {
            return string.Format("sp_rename N'{0}', N'{1}'", oldName, newName);
        }

        public string RenameColumn(string oldName, string newName)
        {
            return string.Format("sp_rename N'{0}', N'{1}', 'COLUMN'", oldName, newName);
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