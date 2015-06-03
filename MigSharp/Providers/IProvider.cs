using System.Collections.Generic;
using System.Data;

namespace MigSharp.Providers
{
    /// <summary>
    /// Represents a type that knows how to provide database-specific DDL statements.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        /// Checks whether a user created table exists on the database. The returned SQL command must yield 0 if the table does not exist.
        /// </summary>
        /// <param name="databaseName">The database name.</param>
        /// <param name="tableName">The table name.</param>
        /// <returns>The SQL command to be executed.</returns>
        string ExistsTable(string databaseName, string tableName);

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
        IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName);

        /// <summary>
        /// Drops a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropTable(string tableName, bool checkIfExists);

        /// <summary>
        /// Adds columns to an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddColumn(string tableName, Column column);

        /// <summary>
        /// Renames an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenameTable(string oldName, string newName);

        /// <summary>
        /// Renames a column of an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenameColumn(string tableName, string oldName, string newName);

        /// <summary>
        /// Removes a column from an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropColumn(string tableName, string columnName);

        /// <summary>
        /// Changes the data type of a column.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AlterColumn(string tableName, Column column);

        /// <summary>
        /// Adds an index to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName);

        /// <summary>
        /// Drops an index from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropIndex(string tableName, string indexName);

        /// <summary>
        /// Adds a foreign key constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName, bool cascadeOnDelete);

        /// <summary>
        /// Drops a foreign key constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropForeignKey(string tableName, string constraintName);

        /// <summary>
        /// Adds a primary key constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName);

        /// <summary>
        /// Renames the primary key.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> RenamePrimaryKey(string tableName, string oldName, string newName);

        /// <summary>
        /// Drops a primary key constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropPrimaryKey(string tableName, string constraintName);

        /// <summary>
        /// Adds an unique constraint to a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName);

        /// <summary>
        /// Drops a unique constraint from a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropUniqueConstraint(string tableName, string constraintName);

        /// <summary>
        /// Drops the default value (constraint) from a column.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropDefault(string tableName, Column column);
    }
}