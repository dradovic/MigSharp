namespace MigSharp
{
    /// <summary>
    /// Represents a unique constraint which is about to be added to a table.
    /// </summary>
    public interface IAddedUniqueConstraint
    {
        /// <summary>
        /// Adds the unique constraint on the provided column.
        /// </summary>
        IAddedUniqueConstraint OnColumn(string columnName);
    }
}