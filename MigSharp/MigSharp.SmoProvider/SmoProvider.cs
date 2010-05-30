using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Microsoft.SqlServer.Management.Smo;

using MigSharp.Providers;

namespace MigSharp.Smo
{
    public class SmoProvider : IProvider
    {
        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns)
        {
            Table table = new Table(CreateDatabase(), tableName);
            foreach (CreatedColumn createdColumn in columns)
            {
                Column column = new Column(table, createdColumn.Name)
                {
                    DataType = Convert(createdColumn.Type),
                    Nullable = createdColumn.IsNullable,
                };
                table.Columns.Add(column);
            }
            List<CreatedColumn> primaryKeyColumns = new List<CreatedColumn>(columns.Where(c => c.IsPrimaryKey));
            if (primaryKeyColumns.Count > 0)
            {
                Index pkIndex = new Index(table, "PK_" + tableName); // TODO: figure out the default name of the PK constraint
                pkIndex.IndexKeyType = IndexKeyType.DriPrimaryKey;
                foreach (CreatedColumn primaryKeyColumn in primaryKeyColumns)
                {
                    pkIndex.IndexedColumns.Add(new IndexedColumn(pkIndex, primaryKeyColumn.Name));
                }
                table.Indexes.Add(pkIndex);
            }
            ScriptingOptions options = new ScriptingOptions
            {
                PrimaryObject = true,
                Indexes = true,
                SchemaQualify = true
            };
            return table.Script(options).Cast<string>();
        }

        public IEnumerable<string> AddColumns(string tableName, IEnumerable<AddedColumn> columns)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> RenameColumn(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DropDefaultConstraint(string tableName, string constraintName)
        {
            throw new NotImplementedException();
        }

        private static Database CreateDatabase()
        {
            Server server = new Server();
            return new Database(server, "SmoProvider"); // the name of the database does not matter
        }

        private static DataType Convert(DbType dbType)
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
                    break;
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