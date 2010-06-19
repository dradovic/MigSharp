using System.Collections.Generic;

namespace MigSharp.Providers
{
    /// <summary>
    /// Represents a type that knows a database-specific SQL dialect for schema changing statements (DDL).
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        /// Creates a tables with the specified <paramref name="columns"/>
        /// </summary>
        /// <param name="tableName">The name of the new table.</param>
        /// <param name="columns">The columns of the new table.</param>
        /// <param name="onlyIfNotExists">If <c>true</c>, the creation of the table is guarded with an if-exists clause.</param>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> CreateTable(string tableName, IEnumerable<CreatedColumn> columns, bool onlyIfNotExists);

        /// <summary>
        /// Drops a table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropTable(string tableName);

        /// <summary>
        /// Adds columns to an existing table.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> AddColumns(string tableName, IEnumerable<AddedColumn> columns);

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
        /// Drops a default contraint from a column.
        /// </summary>
        /// <returns>The SQL commands to be executed.</returns>
        IEnumerable<string> DropDefaultConstraint(string tableName, string columnName);
    }
}