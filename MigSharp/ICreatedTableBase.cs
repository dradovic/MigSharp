using System.Data;

namespace MigSharp
{
    /// <summary>
    /// Represents a created table.
    /// </summary>
    public interface ICreatedTableBase
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Adds a non-nullable column which is part of the primary key constraint to the table being created.
        /// </summary>
        ICreatedTableWithAddedColumn WithPrimaryKeyColumn(string columnName, DbType type);

        /// <summary>
        /// Adds a non-nullable column to the table being created.
        /// </summary>
        ICreatedTableWithAddedColumn WithNotNullableColumn(string columnName, DbType type);

        /// <summary>
        /// Adds a nullable column to the table being created.
        /// </summary>
        ICreatedTableWithAddedColumn WithNullableColumn(string columnName, DbType type);

        /// <summary>
        /// Adds a row-version column. ATTENTION: this feature is only supported by SQL Server and SQL Server CE.
        /// </summary>
        ICreatedTableWithAddedColumn WithRowVersionColumn(string columnName);

    }
}