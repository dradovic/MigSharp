namespace MigSharp
{
    /// <summary>
    /// Represents a primary key constraint which is about to be added to a table.
    /// </summary>
    public interface IAddedPrimaryKey
    {
        /// <summary>
        /// Adds the index on the provided column.
        /// </summary>
        IAddedPrimaryKey OnColumn(string columnName);
    }
}