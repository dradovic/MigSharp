using System.Data;

namespace MigSharp
{
    /// <summary>
    /// Represents an existing column.
    /// </summary>
    public interface IExistingColumn
    {
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Gets the name of the table of the column.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Renames the column.
        /// </summary>
        void Rename(string newName);

        /// <summary>
        /// Removes the column from its table.
        /// </summary>
        void Drop();

        /// <summary>
        /// Alters the column to be a nullable column.
        /// </summary>
        /// <param name="dbType">A new or the old data type of the column.</param>
        IAlteredColumn AlterToNullable(DbType dbType);

        /// <summary>
        /// Alters the column to be a nullable column.
        /// </summary>
        /// <param name="dbType">A new or the old data type of the column.</param>
        IAlteredColumn AlterToNotNullable(DbType dbType);
    }
}