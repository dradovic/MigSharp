using System;
using System.Collections.Generic;

namespace MigSharp.Providers
{
    [ProviderExport(ProviderNames.SQLite, InvariantName, MaximumDbObjectNameLength = 128)]
    internal class SQLiteProvider : IProvider
    {
        public const string InvariantName = "System.Data.SQLite";

        public string ExistsTable(string databaseName, string tableName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DropTable(string tableName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AddColumn(string tableName, AddedColumn column)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> RenameTable(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> RenameColumn(string tableName, string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DropColumn(string tableName, string columnName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AlterColumn(string tableName, Column column)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DropIndex(string tableName, string indexName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DropForeignKeyConstraint(string tableName, string constraintName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DropPrimaryKeyConstraint(string tableName, string constraintName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName)
        {
            throw new NotImplementedException();
        }
    }
}