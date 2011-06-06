using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

using MigSharp.Providers;

using Column = MigSharp.Providers.Column;
using DataType = MigSharp.Providers.DataType;

namespace MigSharp.SqlServer.NUnit
{
    public class SmoProvider : IProvider
    {
        public static string InvariantName { get { return "System.Data.SqlClient.ForBackTestingOnly"; } }
        public const int MaximumDbObjectNameLength = SqlServerProviderBase.MaximumDbObjectNameLength;

        public string ExistsTable(string databaseName, string tableName)
        {
            throw new NotSupportedException();
        }

        public string ConvertToSql(object value, DbType targetDbType)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            Table table = GetTable(tableName);
            foreach (CreatedColumn createdColumn in columns)
            {
                var column = new Microsoft.SqlServer.Management.Smo.Column(table, createdColumn.Name)
                {
                    DataType = Convert(createdColumn.DataType),
                    Nullable = createdColumn.IsNullable,
                    Identity = createdColumn.IsIdentity,
                };
                if (createdColumn.DefaultValue != null)
                {
                    AddDefaultConstraint(column, tableName, createdColumn.DefaultValue);
                }
                table.Columns.Add(column);
            }
            List<CreatedColumn> primaryKeyColumns = new List<CreatedColumn>(columns.Where(c => c.IsPrimaryKey));
            if (primaryKeyColumns.Count > 0)
            {
                Index pkIndex = new Index(table, primaryKeyConstraintName);
                pkIndex.IndexKeyType = IndexKeyType.DriPrimaryKey;
                foreach (CreatedColumn primaryKeyColumn in primaryKeyColumns)
                {
                    pkIndex.IndexedColumns.Add(new IndexedColumn(pkIndex, primaryKeyColumn.Name));
                }
                table.Indexes.Add(pkIndex);
            }
            foreach (var uniqueColumns in columns
                .Where(c => !string.IsNullOrEmpty(c.UniqueConstraint))
                .GroupBy(c => c.UniqueConstraint))
            {
                Index uniqueIndex = new Index(table, uniqueColumns.Key);
                uniqueIndex.IndexKeyType = IndexKeyType.DriUniqueKey;
                foreach (var column in uniqueColumns)
                {
                    uniqueIndex.IndexedColumns.Add(new IndexedColumn(uniqueIndex, column.Name));
                }
                table.Indexes.Add(uniqueIndex);
            }
            table.Create();
            ScriptingOptions options = new ScriptingOptions { Indexes = true, DriDefaults = true };
            return TransformScript(table.Script(options));
        }

        public IEnumerable<string> DropTable(string tableName)
        {
            Table table = GetTable(tableName);
            table.Drop();
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> AddColumn(string tableName, Column column)
        {
            Table table = GetTable(tableName);
            var c = new Microsoft.SqlServer.Management.Smo.Column(table, column.Name)
            {
                DataType = Convert(column.DataType),
                Nullable = column.IsNullable,
            };
            if (column.DefaultValue != null)
            {
                AddDefaultConstraint(c, tableName, column.DefaultValue);
            }
            table.Columns.Add(c);
            table.Alter();
            return ScriptChanges(table.Parent.Parent);
        }

        private static DefaultConstraint AddDefaultConstraint(Microsoft.SqlServer.Management.Smo.Column column, string tableName, object value)
        {
            string constraintName = ObjectNameHelper.GetObjectName(tableName, "DF", MaximumDbObjectNameLength, column.Name);
            DefaultConstraint defaultConstraint = column.AddDefaultConstraint(constraintName);
            defaultConstraint.Text = GetDefaultValueAsString(value);
            return defaultConstraint;
        }

        private static string GetDefaultValueAsString(object value)
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
                return SqlScriptingHelper.ToSql(value, DbType.DateTime);
            }
            else if (value is string)
            {
                return SqlScriptingHelper.ToSql(value, DbType.String);
            }
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            Table table = GetTable(oldName);
            table.Rename(newName);
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            Table table = GetTable(tableName);
            var column = new Microsoft.SqlServer.Management.Smo.Column(table, oldName);

            // rename default constraint
            string oldDefaultConstraintName = ObjectNameHelper.GetObjectName(tableName, "DF", MaximumDbObjectNameLength, oldName);
            string constraintName = oldDefaultConstraintName;
            DefaultConstraint defaultConstraint = column.AddDefaultConstraint(constraintName);
            defaultConstraint.Rename(ObjectNameHelper.GetObjectName(tableName, "DF", MaximumDbObjectNameLength, newName));

            // rename column
            table.Columns.Add(column);
            column.Rename(newName);

            // script changes
            IEnumerable<string> commandTexts = ScriptChanges(table.Parent.Parent);
            string renameDefaultConstraintCommandText = string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID('{0}') IS NOT NULL ", oldDefaultConstraintName) + commandTexts.First();
            yield return Regex.Replace(renameDefaultConstraintCommandText, @"EXEC \[\w+\]\.dbo\.sp_rename", @"EXEC dbo.sp_rename"); // for some reason SMO is putting the server name in front of dbo.sp_rename which we do not have in the SqlServerProvider
            foreach (string commandText in commandTexts.Skip(1))
            {
                yield return commandText;
            }
        }

        public IEnumerable<string> DropColumn(string tableName, string columnName)
        {
            Table table = GetTable(tableName);
            var column = new Microsoft.SqlServer.Management.Smo.Column(table, columnName);
            table.Columns.Add(column);
            column.Drop();
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> AlterColumn(string tableName, Column column)
        {
            // drop (possibly) existing default constraint
            Table table = GetTable(tableName);
            var c = new Microsoft.SqlServer.Management.Smo.Column(table, column.Name)
            {
                Nullable = column.IsNullable,
                DataType = Convert(column.DataType),
            };
            DefaultConstraint defaultConstraint = AddDefaultConstraint(c, tableName, "dummy");
            defaultConstraint.Drop();
            foreach (string command in ScriptChanges(table.Parent.Parent))
            {
                yield return string.Format(CultureInfo.InvariantCulture, "IF OBJECT_ID('{0}') IS NOT NULL ", defaultConstraint.Name) + command;
            }

            // alter column
            table = GetTable(tableName);
            c = new Microsoft.SqlServer.Management.Smo.Column(table, column.Name)
            {
                Nullable = column.IsNullable,
                DataType = Convert(column.DataType),
            };
            if (column.DefaultValue != null)
            {
                AddDefaultConstraint(c, tableName, column.DefaultValue);
            }
            c.Alter();
            foreach (string command in ScriptChanges(table.Parent.Parent))
            {
                yield return command;
            }
        }

        public IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName)
        {
            Table table = GetTable(tableName);
            Index index = new Index(table, indexName);
            table.Indexes.Add(index);
            foreach (string columnName in columnNames)
            {
                var column = new Microsoft.SqlServer.Management.Smo.Column(table, columnName)
                {
                    DataType = Microsoft.SqlServer.Management.Smo.DataType.Variant
                };
                table.Columns.Add(column);
                IndexedColumn indexedColumn = new IndexedColumn(index, columnName);
                index.IndexedColumns.Add(indexedColumn);
            }
            index.Create();
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> DropIndex(string tableName, string indexName)
        {
            Table table = GetTable(tableName);
            Index index = new Index(table, indexName);
            index.Drop();
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName)
        {
            Table table = GetTable(tableName);
            ForeignKey foreignKey = new ForeignKey(table, constraintName) { ReferencedTable = referencedTableName };
            foreach (ColumnReference columnReference in columnNames)
            {
                var fromColumn = new Microsoft.SqlServer.Management.Smo.Column(table, columnReference.ColumnName);
                table.Columns.Add(fromColumn);
                var foreignKeyColumn = new ForeignKeyColumn(foreignKey, columnReference.ColumnName, columnReference.ReferencedColumnName);
                foreignKey.Columns.Add(foreignKeyColumn);
            }
            foreignKey.Create();
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> DropForeignKey(string tableName, string constraintName)
        {
            Table table = GetTable(tableName);
            ForeignKey foreignKey = new ForeignKey(table, constraintName);
            foreignKey.Drop();
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, IndexKeyType.DriPrimaryKey, columnNames);
        }

        public IEnumerable<string> RenamePrimaryKey(string tableName, string oldName, string newName)
        {
            Table table = GetTable(tableName);
            Index primaryKey = new Index(table, oldName) { IndexKeyType = IndexKeyType.DriPrimaryKey };
            primaryKey.Rename(newName);
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> DropPrimaryKey(string tableName, string constraintName)
        {
            return DropConstraint(tableName, constraintName, IndexKeyType.DriPrimaryKey);
        }

        public IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            return AddConstraint(tableName, constraintName, IndexKeyType.DriUniqueKey, columnNames);
        }

        private static IEnumerable<string> AddConstraint(string tableName, string constraintName, IndexKeyType keyType, IEnumerable<string> columnNames)
        {
            Table table = GetTable(tableName);
            Index uniqueConstraint = new Index(table, constraintName) { IndexKeyType = keyType };
            foreach (string columnName in columnNames)
            {
                Microsoft.SqlServer.Management.Smo.Column column = new Microsoft.SqlServer.Management.Smo.Column(table, columnName);
                column.DataType = Microsoft.SqlServer.Management.Smo.DataType.Bit;
                table.Columns.Add(column);
                uniqueConstraint.IndexedColumns.Add(new IndexedColumn(uniqueConstraint, columnName));
            }
            uniqueConstraint.Create();
            return ScriptChanges(table.Parent.Parent);
        }

        public IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName)
        {
            return DropConstraint(tableName, constraintName, IndexKeyType.DriUniqueKey);
        }

        public IEnumerable<string> DropDefault(string tableName, Column column)
        {
            Table table = GetTable(tableName);
            var c = new Microsoft.SqlServer.Management.Smo.Column(table, column.Name)
            {
                DataType = Convert(column.DataType),
                Nullable = column.IsNullable,
            };
            DefaultConstraint defaultConstraint = AddDefaultConstraint(c, tableName, column.DefaultValue);
            defaultConstraint.Drop();
            return ScriptChanges(table.Parent.Parent);
        }

        private static IEnumerable<string> DropConstraint(string tableName, string constraintName, IndexKeyType keyType)
        {
            Table table = GetTable(tableName);
            Index uniqueConstraint = new Index(table, constraintName) { IndexKeyType = keyType };
            uniqueConstraint.Drop();
            return ScriptChanges(table.Parent.Parent);
        }

        private static Table GetTable(string tableName)
        {
            return new Table(GetDatabase(), tableName);
        }

        private static Database GetDatabase()
        {
            Server server = new Server();
            Database database;
            if (!server.Databases.Contains("MigSharp_SmoProvider"))
            {
                database = new Database(server, "MigSharp_SmoProvider");
                database.Create();
            }
            else
            {
                database = server.Databases["MigSharp_SmoProvider"];
            }
            server.ConnectionContext.SqlExecutionModes = SqlExecutionModes.CaptureSql;
            return database;
        }

        private static IEnumerable<string> ScriptChanges(Server server)
        {
            IEnumerable<string> transformedScript = TransformScript(server.ConnectionContext.CapturedSql.Text);
            return transformedScript;
        }

        private static IEnumerable<string> TransformScript(StringCollection script)
        {
            Trace.WriteLine(script.Cast<string>().Aggregate((s1, s2) => s1 + Environment.NewLine + s2));
            return script.Cast<string>()
                .Where(c => !c.StartsWith("USE ", StringComparison.OrdinalIgnoreCase))
                .Select(c =>
                    {
                        // some SMO statements are prefixed with a comment like: /**** bla bla ****/\r\n
                        string endOfComment = "*/" + Environment.NewLine;
                        int endOfCommentIndex = c.IndexOf(endOfComment, StringComparison.OrdinalIgnoreCase);
                        if (endOfCommentIndex > 0)
                        {
                            return c.Substring(endOfCommentIndex + endOfComment.Length);
                        }
                        return c;
                    });
        }

        private static Microsoft.SqlServer.Management.Smo.DataType Convert(DataType type)
        {
            switch (type.DbType)
            {
                case DbType.AnsiString:
                    if (type.Size > 0)
                    {
                        return Microsoft.SqlServer.Management.Smo.DataType.VarChar(type.Size);
                    }
                    else
                    {
                        return Microsoft.SqlServer.Management.Smo.DataType.VarCharMax;
                    }
                //case DbType.Binary:
                //    break;
                //case DbType.Byte:
                //    break;
                //case DbType.Boolean:
                //    break;
                //case DbType.Currency:
                //    break;
                //case DbType.Date:
                //    break;
                case DbType.DateTime:
                    return Microsoft.SqlServer.Management.Smo.DataType.DateTime;
                case DbType.Decimal:
                    return Microsoft.SqlServer.Management.Smo.DataType.Decimal(type.Scale, type.Size);
                //case DbType.Double:
                //    break;
                //case DbType.Guid:
                //    break;
                //case DbType.Int16:
                //    break;
                case DbType.Int32:
                    return Microsoft.SqlServer.Management.Smo.DataType.Int;
                case DbType.Int64:
                    return Microsoft.SqlServer.Management.Smo.DataType.BigInt;
                //case DbType.Object:
                //    break;
                //case DbType.SByte:
                //    break;
                //case DbType.Single:
                //    break;
                case DbType.String:
                    if (type.Size > 0)
                    {
                        return Microsoft.SqlServer.Management.Smo.DataType.NVarChar(type.Size);
                    }
                    else
                    {
                        return Microsoft.SqlServer.Management.Smo.DataType.NVarCharMax;                        
                    }
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
                case DbType.AnsiStringFixedLength:
                    return Microsoft.SqlServer.Management.Smo.DataType.Char(type.Size);
                case DbType.StringFixedLength:
                    return Microsoft.SqlServer.Management.Smo.DataType.NChar(type.Size);
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