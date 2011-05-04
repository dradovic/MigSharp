using System.Data;

namespace MigSharp
{
    /// <summary>
    /// Represents a table that was created before.
    /// </summary>
    public interface IExistingTableBase
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Adds a not-nullable column.
        /// </summary>
        IExistingTableWithAddedColumn AddNotNullableColumn(string name, DbType type);

        /// <summary>
        /// Adds a nullable column.
        /// </summary>
        IExistingTableWithAddedColumn AddNullableColumn(string name, DbType type);
    }
}