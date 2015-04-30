using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.Oracle, "Oracle.DataAccess.Client", MaximumDbObjectNameLength = MaximumDbObjectNameLength, ParameterExpression = ":p")]
    [Supports(DbType.AnsiString, MaximumSize = 4000, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.AnsiString, Warning = "Might require custom ADO.NET code as CLOB has unique restrictions (e.g. columns using this data type cannot appear in a WHERE clause without converting using the Oracle 'to_char' function).")]
    [Supports(DbType.Binary)]
    [Supports(DbType.Byte, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true, Warning = "Requires custom ADO.NET code to convert to/from an Int32 (using System.Convert).")]
    [Supports(DbType.Boolean, CanBeUsedAsPrimaryKey = true, Warning = "Requires custom ADO.NET code to convert to/from an Int32 (using System.Convert).")]
    [Supports(DbType.DateTime, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.Decimal, MaximumSize = 28, MaximumScale = 28, CanBeUsedAsPrimaryKey = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Decimal, MaximumSize = 28, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)] // this is a restriction of the decimal type of the CLR (see http://support.microsoft.com/kb/932288)
    [Supports(DbType.Double)]
    [Supports(DbType.Guid, CanBeUsedAsPrimaryKey = true, Warning = "Requires custom ADO.NET code to convert to/from a byte array (call Guid.ToByteArray(), Guid(byte[])) and the DbParameter.DbType must be set to DbType.Binary.")]
    [Supports(DbType.Int16, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int32, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.Int64, CanBeUsedAsPrimaryKey = true, CanBeUsedAsIdentity = true)]
    [Supports(DbType.String, MaximumSize = 2000, CanBeUsedAsPrimaryKey = true)]
    [Supports(DbType.String, Warning = "Might require custom ADO.NET code as NCLOB has unique restrictions (e.g. columns using this data type cannot appear in a WHERE clause without converting using the Oracle 'to_char' function).")]
    internal class OracleProvider : IProvider
    {
        private const int MaximumDbObjectNameLength = 30;
        private const string Identation = "\t";

        public string ExistsTable(string databaseName, string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"SELECT COUNT(*) FROM ALL_TABLES WHERE TABLE_NAME = '{0}' AND OWNER = USER", tableName);
        }

        public string ConvertToSql(object value, DbType targetDbType)
        {
            if (targetDbType == DbType.DateTime)
            {
                return string.Format(CultureInfo.InvariantCulture, "TO_DATE('{0}','DD-MON-YYYY HH24:MI:SS')",
                    ((DateTime)value).ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            }
            return SqlScriptingHelper.ToSql(value, targetDbType);
        }

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            if (columns.Any(c => c.IsRowVersion))
            {
                throw new NotSupportedException("Oracle does not have a unique auto-increment row-version concept. Instead, create the table with the ROWDEPENDENCIES hind and query for ORA_ROWSCN."); // see: http://stackoverflow.com/questions/20487657/sql-server-rowversion-equivalent-in-oracle
            }

            string commandText = string.Empty;
            string identityColumn = string.Empty;
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
                if (column.IsIdentity)
                {
                    identityColumn = column.Name;
                }

                commandText += GetColumnString(column);

                columnDelimiterIsNeeded = true;
            }

            if (primaryKeyColumns.Count > 0)
            {
                commandText += string.Format(CultureInfo.InvariantCulture, "{0} , CONSTRAINT \"{1}\" PRIMARY KEY {2}",
                    Environment.NewLine,
                    primaryKeyConstraintName,
                    Environment.NewLine);
                commandText += string.Format(CultureInfo.InvariantCulture, "({0}", Environment.NewLine);

                columnDelimiterIsNeeded = false;
                foreach (string column in primaryKeyColumns)
                {
                    if (columnDelimiterIsNeeded) commandText += string.Format(CultureInfo.InvariantCulture, ", {0}", Environment.NewLine);

                    // FEATURE: make sort order configurable
                    commandText += string.Format(CultureInfo.InvariantCulture, "{0} {1} ", Identation, Escape(column));

                    columnDelimiterIsNeeded = true;
                }
                commandText += string.Format(CultureInfo.InvariantCulture, " ) {0}", Environment.NewLine);
            }

            foreach (var uniqueColumns in columns
                .Where(c => !string.IsNullOrEmpty(c.UniqueConstraint))
                .GroupBy(c => c.UniqueConstraint))
            {
                commandText += string.Format(CultureInfo.InvariantCulture, ",{0} CONSTRAINT \"{1}\" UNIQUE {2}",
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

            commandText += string.Format(CultureInfo.InvariantCulture, " ) {0}", Environment.NewLine);

            List<string> comands = new List<string> { commandText };
            if (!String.IsNullOrEmpty(identityColumn))
            {
                string sequenceName = GetSequenceName(tableName);
                string createSequence = string.Format(CultureInfo.InvariantCulture, @"CREATE SEQUENCE ""{0}"" MINVALUE 1 START WITH 1 INCREMENT BY 1 CACHE 20 ORDER NOCYCLE",
                    sequenceName);
                string createTrigger = CreateTrigger(tableName, identityColumn, sequenceName);
                comands = new List<string> { createSequence, commandText, createTrigger };
            }

            return comands;
        }

        private static string CreateTrigger(string tableName, string identityColumn, string sequenceName)
        {
            return string.Format(CultureInfo.InvariantCulture, @"CREATE TRIGGER ""{0}"" BEFORE INSERT ON {1} FOR EACH ROW BEGIN SELECT ""{3}"".NEXTVAL into :new.{2} FROM dual; END;",
                GetTriggerName(tableName),
                Escape(tableName),
                Escape(identityColumn),
                sequenceName);
        }

        private static string GetTriggerName(string tableName)
        {
            return ObjectNameHelper.GetObjectName(tableName, "TRG_SEQ", MaximumDbObjectNameLength, tableName.GetHashCode().ToString(CultureInfo.InvariantCulture));
        }

        private static string GetSequenceName(string tableName)
        {
            return ObjectNameHelper.GetObjectName(tableName, "SEQ", MaximumDbObjectNameLength, tableName.GetHashCode().ToString(CultureInfo.InvariantCulture));
        }

        public IEnumerable<string> DropTable(string tableName, bool checkIfExists)
        {
            if (checkIfExists)
            {
                // see: http://stackoverflow.com/questions/1799128/oracle-if-table-exists
                yield return string.Format(CultureInfo.InvariantCulture, @"BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE {0}';
EXCEPTION
   WHEN OTHERS THEN
      IF SQLCODE != -942 THEN
         RAISE;
      END IF;
END;", Escape(tableName));
            }
            else
            {
                yield return string.Format(CultureInfo.InvariantCulture, @"DROP TABLE {0}", Escape(tableName));
            }

            // drop associated SEQUENCE (if it exists); the TRIGGER is dropped automatically by Oracle
            // Oracle Database Error Code ORA-02289: sequence does not exist
            yield return string.Format(CultureInfo.InvariantCulture, @"BEGIN EXECUTE IMMEDIATE 'DROP SEQUENCE ""{0}""'; EXCEPTION WHEN OTHERS THEN IF SQLCODE = -2289 THEN NULL; ELSE RAISE; END IF; END;", // see: http://frankschmidt.blogspot.com/2009/12/drop-table-if-exists-or-sequence-or.html
                GetSequenceName(tableName));
        }

        private string GetColumnString(Column column)
        {
            string defaultConstraintClause = string.Empty;
            string commandText = string.Empty;
            if (column.DefaultValue != null)
            {
                string defaultValue = GetDefaultValueAsString(column.DefaultValue);

                defaultConstraintClause = string.Format(CultureInfo.InvariantCulture, " DEFAULT {0}", defaultValue);
            }
            commandText += string.Format(CultureInfo.InvariantCulture, "{0} {1} {3} {2}NULL",
                Escape(column.Name),
                GetTypeSpecifier(column.DataType),
                column.IsNullable ? string.Empty : "NOT ",
                defaultConstraintClause);

            return commandText;
        }

        private string GetDefaultValueAsString(object value)
        {
            if (value is SpecialDefaultValue)
            {
                switch ((SpecialDefaultValue)value)
                {
                    case SpecialDefaultValue.CurrentDateTime:
                        return "SYSDATE";
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

        public IEnumerable<string> AddColumn(string tableName, Column column)
        {
            // assemble ALTER TABLE statements
            string commandText = string.Format(CultureInfo.InvariantCulture, @"{0} ADD ", AlterTable(tableName));
            commandText += GetColumnString(column);
            yield return commandText;
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            // rename table
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} RENAME TO {1}", Escape(oldName), Escape(newName));

            // rename sequence if it exists and drop, re-create trigger
            string oldSequenceName = GetSequenceName(oldName);
            string newSequenceName = GetSequenceName(newName);
            string oldTriggerName = GetTriggerName(oldName);
            yield return string.Format(CultureInfo.InvariantCulture,
                @"DECLARE
                    l_idColumn LONG;
                    l_3rdQuotePos INTEGER;
                    l_4thQuotePos INTEGER;
                BEGIN
                    /* try to rename the sequence - if it does not exists, it will throw 2289 */
                    EXECUTE IMMEDIATE 'ALTER SEQUENCE {0} NOCACHE'; /* disable caching while renaming the sequence */
                    EXECUTE IMMEDIATE 'RENAME {0} TO {1}';
                    EXECUTE IMMEDIATE 'ALTER SEQUENCE {1} CACHE 20';

                    /* drop and re-create the trigger */
                    SELECT TRIGGER_BODY INTO l_idColumn
                        FROM USER_TRIGGERS
                        WHERE TRIGGER_NAME = '{4}';
                    l_3rdQuotePos := INSTR(l_idColumn, '""', 1, 3);
                    l_4thQuotePos := INSTR(l_idColumn, '""', 1, 4);
                    l_idColumn := SUBSTR(l_idColumn, l_3rdQuotePos + 1, l_4thQuotePos - l_3rdQuotePos - 1);
                    EXECUTE IMMEDIATE 'DROP TRIGGER {2}';
                    EXECUTE IMMEDIATE '{3}';
                    EXCEPTION WHEN OTHERS THEN IF SQLCODE = -2289 THEN 
                        NULL;
                    ELSE
                        RAISE;
                    END IF;
                END;",
                Escape(oldSequenceName),
                Escape(newSequenceName),
                Escape(oldTriggerName),
                CreateTrigger(newName, "' || l_idColumn || '", newSequenceName),
                oldTriggerName
                ).Replace(Environment.NewLine, " ");
        }

        public IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} RENAME COLUMN {1} TO {2}", Escape(tableName), Escape(oldName), Escape(newName));
        }

        public IEnumerable<string> DropColumn(string tableName, string columnName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP COLUMN {1}", Escape(tableName), Escape(columnName));
        }

        public IEnumerable<string> AlterColumn(string tableName, Column column)
        {
            string query = @"declare 
                             l_nullable varchar2(1);
                             l_datatype varchar2(106);
                            begin
                              select 
                                    nullable into l_nullable
                                    
                              from user_tab_columns
                              where table_name = '{0}'
                              and   column_name = '{1}';

                              select 
                                     data_type into l_datatype
                              from user_tab_columns
                              where table_name = '{0}'
                              and   column_name = '{1}';

                              if l_nullable = 'N' and l_datatype != '{2}' then
                                execute immediate 'alter table ""{0}"" modify (""{1}"" {2} {5} {3}  )';
                              end if;
                              if l_nullable = 'Y' and l_datatype != '{2}' then
                                execute immediate 'alter table ""{0}"" modify (""{1}"" {2}  {5} {4} )';
                              end if;
                              if l_nullable = 'N' and l_datatype = '{2}' then
                                execute immediate 'alter table ""{0}"" modify (""{1}"" {5} {3} )';
                              end if;
                              if l_nullable = 'Y' and l_datatype = '{2}' then
                                execute immediate 'alter table ""{0}"" modify (""{1}"" {5} {4} )';
                              end if;
                            end;";

            query = query.Replace(Environment.NewLine, " ");
            string colN = column.IsNullable ? "NULL" : "";
            string colY = column.IsNullable ? "" : "NOT NULL";
            string defaultConstraintClause = (column.DefaultValue == null) ? "DEFAULT NULL" : string.Format(CultureInfo.InvariantCulture, " DEFAULT {0}", GetDefaultValueAsString(column.DefaultValue).Replace("'", "''"));
            yield return string.Format(CultureInfo.InvariantCulture, query, tableName, column.Name, GetTypeSpecifier(column.DataType), colN, colY, defaultConstraintClause);
        }

        private static IEnumerable<string> DropConstraint(string tableName, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "{0} DROP CONSTRAINT {1}", AlterTable(tableName), Escape(constraintName));
        }

        public IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "CREATE INDEX {0} ON {1} ({2})", Escape(indexName), Escape(tableName), GetCsList(columnNames));
        }

        public IEnumerable<string> DropIndex(string tableName, string indexName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "DROP INDEX {0}", Escape(indexName));
        }

        public IEnumerable<string> AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName)
        {
            string sourceCols = String.Empty;
            string targetCols = String.Empty;

            foreach (ColumnReference cr in columnNames)
            {
                sourceCols += Escape(cr.ColumnName) + ",";
                targetCols += Escape(cr.ReferencedColumnName) + ",";
            }
            sourceCols = sourceCols.TrimEnd(',');
            targetCols = targetCols.TrimEnd(',');

            yield return string.Format(CultureInfo.InvariantCulture, "{0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})", AlterTable(tableName), Escape(constraintName), sourceCols, Escape(referencedTableName), targetCols);
        }

        public IEnumerable<string> DropForeignKey(string tableName, string constraintName)
        {
            return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "{0} ADD CONSTRAINT {1} PRIMARY KEY ({2})", AlterTable(tableName), Escape(constraintName), GetCsList(columnNames));
        }

        public IEnumerable<string> RenamePrimaryKey(string tableName, string oldName, string newName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "{0} RENAME CONSTRAINT {1} TO {2}", AlterTable(tableName), Escape(oldName), Escape(newName));
            yield return string.Format(CultureInfo.InvariantCulture, "{0} RENAME TO {1}", AlterIndex(oldName), Escape(newName));
        }

        private static string AlterIndex(string indexName)
        {
            return string.Format(CultureInfo.InvariantCulture, "ALTER INDEX {0}", Escape(indexName));
        }

        public IEnumerable<string> DropPrimaryKey(string tableName, string constraintName)
        {
            return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "{0} ADD CONSTRAINT {1} UNIQUE ({2})", AlterTable(tableName), Escape(constraintName), GetCsList(columnNames));
        }

        public IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName)
        {
            return DropConstraint(tableName, constraintName);
        }

        public IEnumerable<string> DropDefault(string tableName, Column column)
        {
            Debug.Assert(column.DefaultValue == null, "The DefaultValue must be null as we are going to call AlterColumn with it.");
            return AlterColumn(tableName, column);
        }

        private static string CreateTable(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}", Escape(tableName));
        }

        private static string AlterTable(string tableName)
        {
            return string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0}", Escape(tableName));
        }

        private static string Escape(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, "\"{0}\"", name);
        }

        private static string GetTypeSpecifier(DataType type)
        {
            switch (type.DbType)
            {
                case DbType.AnsiString:
                    if (type.Size.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "VARCHAR2({0})", type.Size);
                    }
                    return "CLOB";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Byte:
                    return "NUMBER(3)";
                case DbType.Boolean:
                    return "NUMBER(1)"; // FEATURE: add a constraint to only allow 0/1
                    //case DbType.Currency:
                    //    break;
                    //case DbType.Date:
                    //    break;
                case DbType.DateTime:
                    return "DATE";
                case DbType.Decimal:
                    return "DECIMAL(" + type.Size + "," + (type.Scale ?? 0) + ")";
                case DbType.Double:
                    return "DOUBLE PRECISION";
                case DbType.Guid:
                    return "RAW(16)";
                case DbType.Int16:
                    return "NUMBER(5)";
                case DbType.Int32:
                    return "INTEGER";
                case DbType.Int64:
                    return "NUMBER(19)";
                    //case DbType.Object:
                    //    break;
                    //case DbType.SByte:
                    //    break;
                    //case DbType.Single:
                    //    break;
                case DbType.String:
                    if (type.Size.HasValue)
                    {
                        return string.Format(CultureInfo.InvariantCulture, "NVARCHAR2({0})", type.Size);
                    }
                    return "NCLOB";
                    //case DbType.Time:
                    //    break;
                    //case DbType.UInt16:
                    //    break;
                    //case DbType.UInt32:
                    //    break;
                    //case DbType.UInt64:
                    //    break;
                    //case DbType.VarNumeric:
                    //    break;
                    //case DbType.AnsiStringFixedLength:
                    //    break;
                    //case DbType.StringFixedLength:
                    //    break;
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

        private static string GetCsList(IEnumerable<string> columnNames)
        {
            string columns = String.Empty;
            foreach (var column in columnNames)
            {
                columns += Escape(column) + ",";
            }
            columns = columns.TrimEnd(',');

            return columns;
        }
    }
}