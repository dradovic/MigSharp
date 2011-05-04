namespace MigSharp
{
    /// <summary>
    /// Represents an index which is about to be added to a table.
    /// </summary>
    public interface IAddedIndex
    {
        /// <summary>
        /// Adds the index on the provided column.
        /// </summary>
        IAddedIndex OnColumn(string columnName);
    }
}