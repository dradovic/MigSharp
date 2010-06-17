using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;

using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

using MigSharp.Providers;

namespace MigSharp.Smo
{
    public class SmoProvider : IProvider
    {
        private readonly Server _server = new Server();

        public string InvariantName { get { return "System.Data.SqlClient.ForBackTestingOnly"; } }

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, bool onlyIfNotExists)
        {
            Table table = GetTable(tableName);
            foreach (CreatedColumn createdColumn in columns)
            {
                Column column = new Column(table, createdColumn.Name)
                {
                    DataType = Convert(createdColumn.DbType, createdColumn.Length),
                    Nullable = createdColumn.IsNullable,
                };
                table.Columns.Add(column);
            }
            List<CreatedColumn> primaryKeyColumns = new List<CreatedColumn>(columns.Where(c => c.IsPrimaryKey));
            if (primaryKeyColumns.Count > 0)
            {
                Index pkIndex = new Index(table, "PK_" + tableName);
                pkIndex.IndexKeyType = IndexKeyType.DriPrimaryKey;
                foreach (CreatedColumn primaryKeyColumn in primaryKeyColumns)
                {
                    pkIndex.IndexedColumns.Add(new IndexedColumn(pkIndex, primaryKeyColumn.Name));
                }
                table.Indexes.Add(pkIndex);
            }
            table.Create();
            ScriptingOptions options = new ScriptingOptions();
            options.IncludeIfNotExists = onlyIfNotExists;
            options.Indexes = true;
            return TransformScript(table.Script(options));
        }

        public IEnumerable<string> AddColumns(string tableName, IEnumerable<AddedColumn> columns)
        {
            Table table = GetTable(tableName);
            List<DefaultConstraint> defaultConstraints = new List<DefaultConstraint>();
            foreach (AddedColumn addedColumn in columns)
            {
                Column column = new Column(table, addedColumn.Name)
                {
                    DataType = Convert(addedColumn.DbType, addedColumn.Length),
                    Nullable = addedColumn.IsNullable,
                };
                if (addedColumn.DefaultValue != null)
                {
                    DefaultConstraint defaultConstraint = column.AddDefaultConstraint();
                    defaultConstraint.Text = addedColumn.DefaultValue.ToString();
                    defaultConstraints.Add(defaultConstraint);
                }
                table.Columns.Add(column);
            }
            table.Alter();
            foreach (DefaultConstraint defaultConstraint in defaultConstraints)
            {
                defaultConstraint.Drop();
            }
            return ScriptChanges();
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            Table table = GetTable(oldName);
            table.Rename(newName);
            return ScriptChanges();
        }

        public IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            Table table = GetTable(tableName);
            Column column = new Column(table, oldName);
            table.Columns.Add(column);
            column.Rename(newName);
            return ScriptChanges();
        }

        public IEnumerable<string> DropDefaultConstraint(string tableName, string columnName)
        {
            Table table = GetTable(tableName);
            Column column = new Column(table, columnName);
            DefaultConstraint defaultConstraint = column.AddDefaultConstraint();
            defaultConstraint.Drop();
            return ScriptChanges();
        }

        private Table GetTable(string tableName)
        {
            return new Table(GetDatabase(), tableName);
        }

        private Database GetDatabase()
        {
            Database database;
            if (!_server.Databases.Contains("MigSharp_SmoProvider"))
            {
                database = new Database(_server, "MigSharp_SmoProvider");
                database.Create();
            }
            else
            {
                database = _server.Databases["MigSharp_SmoProvider"];
            }
            _server.ConnectionContext.SqlExecutionModes = SqlExecutionModes.CaptureSql;
            return database;
        }

        private IEnumerable<string> ScriptChanges()
        {
            return TransformScript(_server.ConnectionContext.CapturedSql.Text);
        }

        private static IEnumerable<string> TransformScript(StringCollection script)
        {
            Trace.WriteLine(script.Cast<string>().Aggregate((s1, s2) => s1 + Environment.NewLine + s2));
            return script.Cast<string>().Where(c => !c.StartsWith("USE "));            
        }

        private static DataType Convert(DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    break;
                case DbType.Binary:
                    break;
                case DbType.Byte:
                    break;
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
                    return DataType.Int;
                case DbType.Int64:
                    break;
                case DbType.Object:
                    break;
                case DbType.SByte:
                    break;
                case DbType.Single:
                    break;
                case DbType.String:
                    return DataType.NVarCharMax;
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
                    return DataType.NVarChar(length);
                case DbType.Xml:
                    break;
                case DbType.DateTime2:
                    break;
                case DbType.DateTimeOffset:
                    break;
            }
            throw new ArgumentOutOfRangeException("dbType"); // TODO: specify all
        }
    }
}