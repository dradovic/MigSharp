using System.Collections.Generic;
using System.Data;
using MigSharp.Core;

namespace MigSharp.Providers
{
    /// <summary>
    /// Represents a type that knows how to provide database-specific DDL statements.
    /// </summary>
    internal interface IProvider
    {
        /// <summary>
        /// Checks whether a user created table exists on the database. The returned SQL command must yield 0 if the table does not exist.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>The SQL command to be executed.</returns>
        string ExistsTable(string databaseName, TableName tableName);

        /// <summary>
        /// Converts an object to its SQL representation for scripting.
        /// </summary>
        string ConvertToSql(object value, DbType targetDbType);

        /// <summary>
        /// Creates a tables with the specified <paramref name="columns"/>
        /// </summary>
        /// <param name="tableName">The name of the new table.</param>
        /// <param name="columns">The columns of the new table.</param>
        /// <param name="primaryKeyConstraintName">Empty if there are no primary key columns.</param>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> CreateTable(TableName tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName);

        /// <summary>
        /// Drops a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropTable(TableName tableName, bool checkIfExists);

        /// <summary>
        /// Adds columns to an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddColumn(TableName tableName, Column column);

        /// <summary>
        /// Renames an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenameTable(TableName oldName, string newName);

        /// <summary>
        /// Renames a column of an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenameColumn(TableName tableName, string oldName, string newName);

        /// <summary>
        /// Removes a column from an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropColumn(TableName tableName, string columnName);

        /// <summary>
        /// Changes the data type of a column.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AlterColumn(TableName tableName, Column column);

        /// <summary>
        /// Adds an index to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddIndex(TableName tableName, IEnumerable<string> columnNames, string indexName);

        /// <summary>
        /// Drops an index from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropIndex(TableName tableName, string indexName);

        /// <summary>
        /// Adds a foreign key constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddForeignKey(TableName tableName, TableName referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName, bool cascadeOnDelete);

        /// <summary>
        /// Drops a foreign key constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropForeignKey(TableName tableName, string constraintName);

        /// <summary>
        /// Adds a primary key constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddPrimaryKey(TableName tableName, IEnumerable<string> columnNames, string constraintName);

        /// <summary>
        /// Renames the primary key.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenamePrimaryKey(TableName tableName, string oldName, string newName);

        /// <summary>
        /// Drops a primary key constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropPrimaryKey(TableName tableName, string constraintName);

        /// <summary>
        /// Adds an unique constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddUniqueConstraint(TableName tableName, IEnumerable<string> columnNames, string constraintName);

        /// <summary>
        /// Drops a unique constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropUniqueConstraint(TableName tableName, string constraintName);

        /// <summary>
        /// Drops the default value (constraint) from a column.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropDefault(TableName tableName, Column column);

        /// <summary>
        /// Creates a database schema.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> CreateSchema(string schemaName);

        /// <summary>
        /// Drops a database schema.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropSchema(string schemaName);
    }
}